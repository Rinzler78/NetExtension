#!/usr/bin/env perl

use strict;
use warnings;
use utf8;

binmode STDOUT, ':encoding(UTF-8)';
binmode STDERR, ':encoding(UTF-8)';

sub print_help {
    print "Usage: changelog-extract.pl [--file <path>] [--version <version>] [--output <path>]\n";
}

my %options = (
    file => undef,
    version => undef,
    output => undef,
);

while (@ARGV) {
    my $arg = shift @ARGV;
    if ($arg eq '--help' || $arg eq '-h') {
        print_help();
        exit 0;
    }

    if ($arg =~ /^--(file|version|output)$/) {
        my $key = $1;
        die "Missing value for $arg\n" if !@ARGV;
        $options{$key} = shift @ARGV;
        next;
    }

    die "Unknown option: $arg\n";
}

die "Missing required options: --file and --version\n"
    if !defined $options{file} || !defined $options{version};

open my $fh, '<:encoding(UTF-8)', $options{file} or die "Cannot read $options{file}: $!\n";
local $/;
my $text = <$fh>;
close $fh;

my $version = quotemeta($options{version});
my $result;
if ($text =~ /(?ms)^## \[$version\].*?(?=^## \[|\z)/) {
    $result = $&;
    $result =~ s/^\s+|\s+$//g;
}
else {
    $result = "## [$options{version}]\n\nNo CHANGELOG entry found for this version.";
}

if (defined $options{output}) {
    open my $out, '>:encoding(UTF-8)', $options{output} or die "Cannot write $options{output}: $!\n";
    print {$out} $result, "\n";
    close $out;
}
else {
    print $result, "\n";
}

exit 0;
