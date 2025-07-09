#!/usr/bin/env python3
"""
Pre-commit hook to check that all comments and documentation are in English.
This script scans C# files for comments that might contain French text.
"""

import re
import sys
import argparse
from pathlib import Path
from typing import List, Tuple, Set

# French words that are clearly French and shouldn't appear in English comments
# Excluding words that are commonly used in both languages (like "test", "type", "note", etc.)
FRENCH_WORDS = {
    'et', 'ou', 'de', 'du', 'le', 'la', 'les', 'des', 'un', 'une', 'dans', 'sur', 'avec', 'pour', 'par', 'sans',
    'est', 'sont', 'était', 'étaient', 'sera', 'seront', 'avoir', 'être', 'fait', 'faire', 'dit', 'dire',
    'tout', 'tous', 'toute', 'toutes', 'très', 'plus', 'moins', 'aussi', 'ainsi', 'donc', 'alors', 'mais',
    'qui', 'que', 'quoi', 'où', 'quand', 'comment', 'pourquoi', 'si', 'comme', 'après', 'avant', 'pendant',
    'cette', 'ces', 'ce', 'ceci', 'cela', 'ça', 'celui', 'celle', 'ceux', 'celles', 'même', 'autres',
    'quelque', 'quelques', 'chaque', 'plusieurs', 'beaucoup', 'peu', 'assez', 'trop', 'encore', 'déjà',
    'ici', 'là', 'partout', 'nulle', 'jamais', 'toujours', 'parfois', 'souvent', 'maintenant', 'aujourd',
    'hier', 'demain', 'bientôt', 'tard', 'tôt', 'actuellement', 'récemment', 'prochainement',
    'données', 'résultats', 'méthode', 'fonction', 'classe', 'propriété', 'paramètre',
    'retour', 'valeur', 'objet', 'fichier', 'dossier', 'répertoire', 'chemin', 'nom', 'taille',
    'créer', 'créé', 'créée', 'créés', 'créées', 'modifier', 'modifié', 'modifiée', 'modifiés', 'modifiées',
    'supprimer', 'supprimé', 'supprimée', 'supprimés', 'supprimées', 'ajouter', 'ajouté', 'ajoutée',
    'vérifier', 'vérifié', 'vérifiée', 'contrôler', 'contrôlé', 'contrôlée', 'initialiser', 'initialisé',
    'utiliser', 'utilisé', 'utilisée', 'utilisés', 'utilisées', 'traiter', 'traité', 'traitée', 'traités',
    'exemple', 'exemples', 'erreur', 'erreurs', 'exception', 'exceptions', 'message',
    'messages', 'information', 'informations', 'donnée', 'résultat', 'réponse',
    'réponses', 'demande', 'demandes', 'requête', 'requêtes', 'opération', 'opérations', 'action', 'actions',
    'processus', 'procédure', 'procédures', 'algorithme', 'algorithmes', 'logique', 'calcul', 'calculs',
    'nombre', 'nombres', 'numéro', 'numéros', 'identifiant', 'identifiants', 'référence', 'références',
    'système', 'systèmes', 'programme', 'programmes', 'logiciel', 'logiciels',
    'développement', 'développements', 'implémentation', 'implémentations', 'version', 'versions',
    'bibliothèque', 'bibliothèques', 'module', 'modules', 'composant', 'composants', 'service', 'services',
    'interface', 'interfaces', 'utilisateur', 'utilisateurs', 'client', 'clients', 'serveur', 'serveurs',
    'réseau', 'réseaux', 'connexion', 'connexions', 'communication', 'communications', 'protocole', 'protocoles',
    'sécurité', 'sécurisé', 'sécurisée', 'sécurisés', 'sécurisées', 'authentification', 'autorisation',
    'permission', 'permissions', 'accès', 'droits', 'privilège', 'privilèges',
    'session', 'sessions', 'cookie', 'cookies', 'cache', 'caches', 'mémoire', 'mémoires', 'stockage',
    'base', 'bases', 'table', 'tables', 'colonne', 'colonnes', 'ligne', 'lignes', 'enregistrement',
    'enregistrements', 'champ', 'champs', 'clé', 'clés', 'index', 'indices', 'requête', 'requêtes',
    'transaction', 'transactions', 'sauvegarde', 'sauvegardes', 'restauration', 'restaurations',
    'journal', 'journaux', 'historique', 'historiques', 'trace', 'traces', 'débogage',
    'performance', 'performances', 'optimisation', 'optimisations', 'amélioration', 'améliorations',
    'correction', 'corrections', 'problème', 'problèmes', 'défaut', 'défauts',
    'solution', 'solutions', 'résolution', 'résolutions', 'réparation', 'réparations', 'maintenance',
    'maintenances', 'aide', 'assistance', 'documentation', 'documentations',
    'manuel', 'manuels', 'guide', 'guides', 'tutoriel', 'tutoriels', 'instruction', 'instructions',
    'étape', 'étapes', 'procédure', 'procédures', 'technique',
    'techniques', 'approche', 'approches', 'stratégie', 'stratégies', 'politique', 'politiques',
    'règle', 'règles', 'convention', 'conventions', 'norme', 'normes',
    'spécification', 'spécifications', 'définition', 'définitions', 'description', 'descriptions',
    'explication', 'explications', 'commentaire', 'commentaires', 'remarque',
    'remarques', 'observation', 'observations', 'analyse', 'analyses', 'étude', 'études',
    'recherche', 'recherches', 'investigation', 'investigations', 'exploration', 'explorations',
    'découverte', 'découvertes', 'invention', 'inventions', 'création', 'créations',
    'évolution', 'évolutions', 'progression', 'progressions', 'avancement',
    'avancements', 'modification', 'modifications', 'changement',
    'changements', 'transformation', 'transformations', 'conversion', 'conversions', 'adaptation',
    'adaptations', 'ajustement', 'ajustements', 'paramétrage',
    'paramétrages', 'réglage', 'réglages', 'calibrage', 'calibrages', 'étalonnage', 'étalonnages'
}

# Pattern to match C# comments
COMMENT_PATTERNS = [
    re.compile(r'//\s*(.+)$', re.MULTILINE),  # Single-line comments
    re.compile(r'/\*\*?(.*?)\*/', re.DOTALL),  # Multi-line comments
    re.compile(r'///\s*(.+)$', re.MULTILINE),  # XML documentation comments
]

def extract_comments(content: str) -> List[Tuple[int, str]]:
    """Extract all comments from C# code with their line numbers."""
    comments = []
    lines = content.split('\n')

    for i, line in enumerate(lines, 1):
        # Check for single-line comments
        match = re.search(r'//\s*(.+)$', line)
        if match:
            comment_text = match.group(1).strip()
            if comment_text:  # Skip empty comments
                comments.append((i, comment_text))

        # Check for XML documentation comments
        match = re.search(r'///\s*(.+)$', line)
        if match:
            comment_text = match.group(1).strip()
            if comment_text and not comment_text.startswith('<'):  # Skip XML tags
                comments.append((i, comment_text))

    # Check for multi-line comments
    for match in re.finditer(r'/\*\*?(.*?)\*/', content, re.DOTALL):
        comment_text = match.group(1).strip()
        if comment_text:
            # Find the line number of the comment
            line_num = content[:match.start()].count('\n') + 1
            # Clean up the comment text
            comment_text = re.sub(r'^\s*\*\s?', '', comment_text, flags=re.MULTILINE)
            comment_text = comment_text.replace('\n', ' ').strip()
            if comment_text:
                comments.append((line_num, comment_text))

    return comments

def check_french_words(text: str) -> Set[str]:
    """Check if text contains French words."""
    words = re.findall(r'\b\w+\b', text.lower())
    return set(words) & FRENCH_WORDS

def check_file(file_path: Path) -> List[Tuple[int, str, Set[str]]]:
    """Check a single file for French comments."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except UnicodeDecodeError:
        return []

    comments = extract_comments(content)
    french_comments = []

    for line_num, comment_text in comments:
        french_words = check_french_words(comment_text)
        if french_words:
            french_comments.append((line_num, comment_text, french_words))

    return french_comments

def main():
    parser = argparse.ArgumentParser(description='Check that comments are in English')
    parser.add_argument('files', nargs='*', help='Files to check')
    args = parser.parse_args()

    if not args.files:
        return 0

    has_errors = False

    for file_path in args.files:
        if not file_path.endswith('.cs'):
            continue

        path = Path(file_path)
        if not path.exists():
            continue

        french_comments = check_file(path)

        if french_comments:
            has_errors = True
            print(f"\n{file_path}:")
            for line_num, comment_text, french_words in french_comments:
                print(f"  Line {line_num}: French words detected: {', '.join(sorted(french_words))}")
                print(f"    Comment: {comment_text}")

    if has_errors:
        print("\nError: Comments should be written in English.")
        print("Please translate the above comments to English.")
        return 1

    return 0

if __name__ == '__main__':
    sys.exit(main())
