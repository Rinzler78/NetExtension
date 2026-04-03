#!/usr/bin/env perl

use strict;
use warnings;
use utf8;
use File::Find;
use File::Path qw(make_path);
use JSON::PP;

binmode STDOUT, ':encoding(UTF-8)';
binmode STDERR, ':encoding(UTF-8)';

sub print_help {
    print "Usage: coverage-report.pl [summary|gate] [options]\n";
}

sub percent {
    my ($covered, $valid) = @_;
    return 100.0 if !$valid;
    return sprintf('%.2f', ($covered / $valid) * 100.0) + 0;
}

sub normalize_file_name {
    my ($file_name) = @_;
    $file_name =~ s{\\}{/}g;
    my $marker = 'src/Rinzler78.NetExtension.Standard/';
    my $index = index($file_name, $marker);
    return $index >= 0 ? substr($file_name, $index + length($marker)) : $file_name;
}

sub find_coverage_files {
    my ($input_path) = @_;
    return ($input_path) if -f $input_path;

    my @files;
    find(
        sub {
            return if !-f $_;
            push @files, $File::Find::name if $_ eq 'coverage.cobertura.xml';
        },
        $input_path,
    );

    @files = sort @files;
    return @files;
}

sub build_summary {
    my (@coverage_files) = @_;
    my %lines_by_file;

    foreach my $coverage_file (@coverage_files) {
        open my $fh, '<:encoding(UTF-8)', $coverage_file or die "Cannot read $coverage_file: $!\n";
        local $/;
        my $content = <$fh>;
        close $fh;

        while ($content =~ m{<class\b[^>]*filename="([^"]+)"[^>]*>(.*?)</class>}sg) {
            my $normalized = normalize_file_name($1);
            my $class_body = $2;

            while ($class_body =~ m{<line\b[^>]*number="(\d+)"[^>]*hits="(\d+)"}g) {
                my ($line_number, $hits) = ($1 + 0, $2 + 0);
                my $previous = $lines_by_file{$normalized}{$line_number};
                if (!defined $previous || $hits > $previous) {
                    $lines_by_file{$normalized}{$line_number} = $hits;
                }
            }
        }
    }

    my @files;
    my $total_valid = 0;
    my $total_covered = 0;

    foreach my $file_name (sort keys %lines_by_file) {
        my $line_hits = $lines_by_file{$file_name};
        my $valid = scalar keys %{$line_hits};
        my $covered = scalar grep { $_ > 0 } values %{$line_hits};
        $total_valid += $valid;
        $total_covered += $covered;
        push @files, {
            file => $file_name,
            covered_lines => $covered,
            valid_lines => $valid,
            line_rate_percent => percent($covered, $valid),
        };
    }

    return {
        global => {
            covered_lines => $total_covered,
            valid_lines => $total_valid,
            line_rate_percent => percent($total_covered, $total_valid),
        },
        files => \@files,
    };
}

sub write_json {
    my ($path, $payload) = @_;
    my ($dir) = $path =~ m{^(.*)/[^/]+$};
    make_path($dir) if defined $dir && length $dir;
    my $json = JSON::PP->new->ascii->pretty->canonical->encode($payload);
    open my $fh, '>:encoding(UTF-8)', $path or die "Cannot write $path: $!\n";
    print {$fh} $json;
    close $fh;
}

sub render_markdown {
    my ($summary, $failures) = @_;
    my @lines = (
        '# Coverage Summary',
        q{},
        sprintf('Global line coverage: **%.2f%%** (%d/%d)', $summary->{global}{line_rate_percent}, $summary->{global}{covered_lines}, $summary->{global}{valid_lines}),
        q{},
        '| File | Coverage | Covered / Valid |',
        '|------|----------|-----------------|',
    );

    foreach my $entry (sort { $a->{line_rate_percent} <=> $b->{line_rate_percent} || $a->{file} cmp $b->{file} } @{$summary->{files}}) {
        push @lines, sprintf('| `%s` | %.2f%% | %d/%d |', $entry->{file}, $entry->{line_rate_percent}, $entry->{covered_lines}, $entry->{valid_lines});
    }

    if ($failures && @{$failures}) {
        push @lines, q{}, '## Gate Failures', q{};
        push @lines, map { "- $_" } @{$failures};
    }

    return join("\n", @lines) . "\n";
}

sub write_markdown {
    my ($path, $summary, $failures) = @_;
    my ($dir) = $path =~ m{^(.*)/[^/]+$};
    make_path($dir) if defined $dir && length $dir;
    open my $fh, '>:encoding(UTF-8)', $path or die "Cannot write $path: $!\n";
    print {$fh} render_markdown($summary, $failures);
    close $fh;
}

sub load_json {
    my ($path) = @_;
    open my $fh, '<:encoding(UTF-8)', $path or die "Cannot read $path: $!\n";
    local $/;
    my $content = <$fh>;
    close $fh;
    return JSON::PP->new->decode($content);
}

sub evaluate_gate {
    my ($summary, $config, $baseline) = @_;
    my @failures;
    my %summary_files = map { $_->{file} => $_ } @{$summary->{files}};
    my %excluded = map { $_ => 1 } @{ $config->{exclusions} // [] };

    if (defined $config->{global_minimum_percent} && $summary->{global}{line_rate_percent} < $config->{global_minimum_percent}) {
        push @failures, sprintf('Global line coverage %.2f%% is below the required %.2f%%', $summary->{global}{line_rate_percent}, $config->{global_minimum_percent});
    }

    if (defined $config->{default_minimum_percent}) {
        foreach my $entry (sort { $a->{file} cmp $b->{file} } @{$summary->{files}}) {
            next if $excluded{$entry->{file}};
            if ($entry->{line_rate_percent} < $config->{default_minimum_percent}) {
                push @failures, sprintf('%s is at %.2f%% and below the default minimum %.2f%%', $entry->{file}, $entry->{line_rate_percent}, $config->{default_minimum_percent});
            }
        }
    }

    foreach my $file_name (sort keys %{ $config->{minimums} // {} }) {
        my $minimum = $config->{minimums}{$file_name};
        my $entry = $summary_files{$file_name};
        if (!$entry) {
            push @failures, "Required coverage file was not found in the report: $file_name";
            next;
        }

        if ($entry->{line_rate_percent} < $minimum) {
            push @failures, sprintf('%s is at %.2f%% and below the required %.2f%%', $file_name, $entry->{line_rate_percent}, $minimum);
        }
    }

    if ($baseline) {
        my %baseline_files = map { $_->{file} => $_ } @{ $baseline->{files} // [] };
        my $tolerance = $config->{baseline_regression_tolerance_percent} // 0.0;
        foreach my $file_name (sort keys %baseline_files) {
            next if $excluded{$file_name};

            my $current_entry = $summary_files{$file_name};
            if (!$current_entry) {
                push @failures, "$file_name is missing from the current coverage report";
                next;
            }

            my $minimum_allowed = $baseline_files{$file_name}{line_rate_percent} - $tolerance;
            if ($current_entry->{line_rate_percent} < $minimum_allowed) {
                push @failures, sprintf('%s regressed from %.2f%% to %.2f%%', $file_name, $baseline_files{$file_name}{line_rate_percent}, $current_entry->{line_rate_percent});
            }
        }
    }

    return \@failures;
}

my $command = shift @ARGV // '';
if (!$command || $command eq '--help' || $command eq '-h') {
    print_help();
    exit 0;
}

die "Unknown command: $command\n" if $command ne 'summary' && $command ne 'gate';

my %options;
while (@ARGV) {
    my $arg = shift @ARGV;
    die "Unknown option: $arg\n" if $arg !~ /^--([a-z\-]+)$/;
    my $key = $1;
    die "Missing value for $arg\n" if !@ARGV;
    $options{$key} = shift @ARGV;
}

die "Missing required option: --input\n" if !defined $options{input};
die "Missing required option: --config\n" if $command eq 'gate' && !defined $options{config};

my @coverage_files = find_coverage_files($options{input});
die "No coverage.cobertura.xml files found under $options{input}\n" if !@coverage_files;

my $summary = build_summary(@coverage_files);

if (defined $options{'json-out'}) {
    write_json($options{'json-out'}, $summary) if $command eq 'summary';
}

if (defined $options{'markdown-out'}) {
    write_markdown($options{'markdown-out'}, $summary, undef) if $command eq 'summary';
}

if ($command eq 'summary') {
    print JSON::PP->new->ascii->pretty->canonical->encode($summary->{global});
    exit 0;
}

my $config = load_json($options{config});
my $baseline = defined $options{baseline} ? load_json($options{baseline}) : undef;
my $failures = evaluate_gate($summary, $config, $baseline);
my $payload = {
    status => @{$failures} ? 'failed' : 'passed',
    summary => $summary,
    failures => $failures,
};

write_json($options{'json-out'}, $payload) if defined $options{'json-out'};
write_markdown($options{'markdown-out'}, $summary, $failures) if defined $options{'markdown-out'};

if (@{$failures}) {
    print STDERR "FAIL: $_\n" for @{$failures};
    exit 1;
}

print "Coverage gate passed\n";
exit 0;
