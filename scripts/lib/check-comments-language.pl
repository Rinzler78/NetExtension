#!/usr/bin/env perl

use strict;
use warnings;
use utf8;

binmode STDOUT, ':encoding(UTF-8)';
binmode STDERR, ':encoding(UTF-8)';

my %FRENCH_WORDS = map { $_ => 1 } qw(
    et ou de du le la les des un une dans sur avec pour par sans
    est sont était étaient sera seront avoir être fait faire dit dire
    tout tous toute toutes très plus moins aussi ainsi donc alors mais
    qui que quoi où quand comment pourquoi si comme après avant pendant
    cette ces ce ceci cela ça celui celle ceux celles même autres
    quelque quelques chaque plusieurs beaucoup peu assez trop encore déjà
    ici là partout nulle jamais toujours parfois souvent maintenant aujourd
    hier demain bientôt tard tôt actuellement récemment prochainement
    données résultats méthode fonction classe propriété paramètre
    retour valeur objet fichier dossier répertoire chemin nom taille
    créer créé créée créés créées modifier modifié modifiée modifiés modifiées
    supprimer supprimé supprimée supprimés supprimées ajouter ajouté ajoutée
    vérifier vérifié vérifiée contrôler contrôlé contrôlée initialiser initialisé
    utiliser utilisé utilisée utilisés utilisées traiter traité traitée traités
    exemple exemples erreur erreurs message messages information informations donnée résultat réponse
    réponses demande demandes requête requêtes opération opérations action actions
    processus procédure procédures algorithme algorithmes logique calcul calculs
    nombre nombres numéro numéros identifiant identifiants référence références
    système systèmes programme programmes logiciel logiciels
    développement développements implémentation implémentations version versions
    bibliothèque bibliothèques module modules composant composants service services
    interface interfaces utilisateur utilisateurs client clients serveur serveurs
    réseau réseaux connexion connexions communication communications protocole protocoles
    sécurité sécurisé sécurisée sécurisés sécurisées authentification autorisation
    permission permissions accès droits privilège privilèges
    session sessions cookie cookies cache caches mémoire mémoires stockage
    base bases table tables colonne colonnes ligne lignes enregistrement
    enregistrements champ champs clé clés transaction transactions sauvegarde sauvegardes restauration restaurations
    journal journaux historique historiques trace traces débogage
    optimisation optimisations amélioration améliorations
    correction corrections problème problèmes défaut défauts
    solution solutions résolution résolutions réparation réparations maintenance maintenances
    aide assistance documentation documentations manuel manuels guide guides tutoriel tutoriels instruction instructions
    étape étapes technique techniques approche approches stratégie stratégies politique politiques
    règle règles convention conventions norme normes
    spécification spécifications définition définitions description descriptions
    explication explications commentaire commentaires remarque remarques observation observations analyse analyses étude études
    recherche recherches investigation investigations exploration explorations
    découverte découvertes invention inventions création créations
    évolution évolutions progression progressions avancement avancements changement changements
    transformation transformations adaptation adaptations ajustement ajustements paramétrage paramétrages
    réglage réglages calibrage calibrages étalonnage étalonnages
);

sub print_help {
    print "Usage: check-comments-language.pl [files ...]\n";
}

sub extract_comments {
    my ($content) = @_;
    my @comments;
    my @lines = split /\n/, $content, -1;

    for (my $index = 0; $index < @lines; $index++) {
        my $line_number = $index + 1;
        my $line = $lines[$index];

        if ($line =~ m{//\s*(.+)$}) {
            my $text = $1;
            $text =~ s/^\s+|\s+$//g;
            push @comments, [$line_number, $text] if length $text;
        }

        if ($line =~ m{///\s*(.+)$}) {
            my $text = $1;
            $text =~ s/^\s+|\s+$//g;
            push @comments, [$line_number, $text] if length $text && $text !~ /^</;
        }
    }

    while ($content =~ m{/\*\*?(.*?)\*/}sg) {
        my $text = $1;
        next if !defined $text;
        my $line_number = substr($content, 0, $-[0]) =~ tr/\n//;
        $line_number += 1;
        $text =~ s/^\s*\*\s?//mg;
        $text =~ s/\r?\n/ /g;
        $text =~ s/^\s+|\s+$//g;
        push @comments, [$line_number, $text] if length $text;
    }

    return @comments;
}

sub find_french_words {
    my ($text) = @_;
    my %found;
    while ($text =~ /\b([[:word:]]+)\b/gu) {
        my $word = lc $1;
        $found{$word} = 1 if $FRENCH_WORDS{$word};
    }

    return sort keys %found;
}

my $first = $ARGV[0] // '';
if ($first eq '--help' || $first eq '-h') {
    print_help();
    exit 0;
}

exit 0 if !@ARGV;

my $has_errors = 0;
foreach my $file_path (@ARGV) {
    next if $file_path !~ /\.cs$/i;
    next if !-e $file_path;

    open my $fh, '<:encoding(UTF-8)', $file_path or next;
    local $/;
    my $content = <$fh>;
    close $fh;

    my @issues;
    foreach my $comment (extract_comments($content)) {
        my ($line_number, $comment_text) = @{$comment};
        my @french_words = find_french_words($comment_text);
        next if !@french_words;
        push @issues, [$line_number, $comment_text, \@french_words];
    }

    next if !@issues;

    $has_errors = 1;
    print "\n$file_path:\n";
    foreach my $issue (@issues) {
        my ($line_number, $comment_text, $french_words) = @{$issue};
        print '  Line ', $line_number, ': French words detected: ', join(', ', @{$french_words}), "\n";
        print '    Comment: ', $comment_text, "\n";
    }
}

if ($has_errors) {
    print "\nError: Comments should be written in English.\n";
    print "Please translate the above comments to English.\n";
    exit 1;
}

exit 0;
