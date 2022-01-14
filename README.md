# Packer

A command line tool for extracting and packing PowerBI template files (PBIT) intended for version/source controlling of power bi models. The contents of the pbit file are extracted and processed for maximum human readability/editability (tables/pages broken up into own files, timestamps removed, json schemas added...). They can then be commited into source control or edited by hand (e.g. in VSCode). After processing manually (e.g. VSCode) or automatically (e.g. git merge) the tool can generate a new pbit file that can be opened in PowerBI.
