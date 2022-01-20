# Packer

Packer is a tool for extracting and packing PowerBI template files (PBIT). It is intended for version/source controlling of PowerBI models. It allows extracting *source code* (data model, measures, M queries, page layouts, themes) from a pbit file and saving them as text files which can be version controlled and/or edited by hand. These files can be assembled back into a new pbit file at any point.

![Model source code loaded in VSCode](https://user-images.githubusercontent.com/9460757/150333744-4fe9c0de-cf4c-466a-9823-76b39f9d2d99.png)

## Background

Out of the box, PowerBI does not offer a good mechanism for versioning and merging of models so users typically resort to versioning models by making separate copies of models for each version. This approach has several drawbacks:

- given two version of a model (two different files) there's no easy way to tell what changed between them.
- there's no easy way to merge changes between multiple people working on the same project (manual process is error prone, time intensive and cpu/memory intensive since both models must be loaded into memory and changes must be copied manually).
- there's no easy way to cherry-pick features from different branches of the same project (e.g. a common project structure with customizations for different clients in separate branches)  
- wasteful of disk space (each file contains the entire model and, potentially, all of the data)

The difficulty with version controlling PowerBI models stems from the inability to access the *source code* of the model (I use the term *source code* a bit loosely here). If the source code (page layouts, table definitions and relationships, measures etc...) could be extracted as text files, they could easily be version controlled, diff-ed and merged using standard tools for version controlling source code such as Git.

That's basically what Packer does. Packer enables extracting the source code of a PowerBI model, as well as assembling a PowerBI model from its source code.

When a model is unpacked it can be stored in a git repository and edited in VSCode. Changes to the model can be tracked by subsequent unpack operations to the same repository. The following image demonstrates viewing a diff between an older version of a model and a new one in VSCode.

![VSCode diff](https://user-images.githubusercontent.com/9460757/150333614-97307650-b88a-4075-9e7d-1cbecafac88d.png)

## PBIT vs PBIX files

Packer works with pbit files (not pbix files). In order to be able to version control a PowerBI model, it must be saved as pbit via the "Save As" command in PowerBI. The reason for using pbit files is that they include a file called `DataModelSchema` which describes the schema of the data model as a json string which can be easily interpreted. In pbix files, on the other hand, the structure of the data model is mixed in with the data inside a `DataModel` file. Aside from the fact that this file includes data (which does not belong in version control) it is a binary file which makes it unsuitable for version control. For this reason, **Packer works with pbit files only**.

## Command line interface

The command line interface for the tool is very simple. It has only two commands: `unpack` and `pack`.

![Packer CLI](https://user-images.githubusercontent.com/9460757/150337177-b8f22c65-0843-4985-906e-e33435d83157.png)

### The unpack command

The `unpack` command unpacks the specified pbit file into the specified folder. The syntax is as follows:

```bash
packer unpack "path\to\pbitfile.pbit" "path\to\repository"
```

The folder parameter is optional. If unspecified, the pbit file will be unpacked into the current folder.

### The pack command

The `pack` command packs a repository folder into a new pbit file. The syntax is as follows:

```bash
packer pack "path\to\repository" "path\to\pbitfile.pbit"
```

The folder parameter is optional. If unspecified, the current folder will be packed into a pbit file.

## The GUI

Packer includes a simple GUI tools as well that allows specifying the path to the pbit file and the path to the repository folder and allows packing and unpacking.

![Packer GUI](https://user-images.githubusercontent.com/9460757/150333538-3b387245-0bf3-4462-bcf5-5d71bb023376.png)

Log messages are displayed inside a textbox. Folder and file paths are remembered between sessions.

## Steps when extracting a pbit file

Pbit files are basically zip archives consisting of several files and folders. When unpacking a pbit file into a folder, Packer unzips its contents and performs a series of steps on the extracted files to ensure maximum readability/editability of the *source code*. The steps that the unpacker perform are described below. I describe them for completeness, though it's not strictly necessary to know or understand these steps in order to use Packer.

### 1. Strip security bindings

This step is needed to allow repacking contents into a pbit file. It consists of deleting the `SecurityBindings` file and removing the corresponding entry from the `[Content_Types].xml` file. This step could technically be performed during packing instead of during extracting, but we don't need the SecurityBindings file in version control so we might as well get rid of it sooner rather than later.

### 2. Strip timestamps

This step strips out `createdTimestamp`, `modifiedTime`, `structureModifiedTime`, `refreshedTime`, `lastUpdate`, `lastSchemaUpdate`, `lastProcessed` from the `DataModelSchema` file. The purpose is to reduce the noise in diffs since timestamps are auto-generated but we don't care about them.

### 3. Extract tables

All tables are extracted from the `DataModelSchema` file into their own json files in the `Tables` subfolder. This makes it easier to figure out which table a change is affecting as well as makes the files smaller and easier to manage.

### 4. Extract measures

DAX queries (measures) are extracted into their own files. These files are saved into the `Measures` subfolder. Once there, they can be edited e.g. in VisualStudio code. This makes editing them much easier because they can be formatted with newlines (json escapes newlines) and with good editor support (VSCode has extensions for editing DAX).

### 5. Extract M queries

M queries are also extracted into their own files which are saved into the `Queries` subfolder. As with DAX queries, extracting them out makes them easier to edit because we can have newlines and also because VSCode has extensions for editing M code.

### 6. Extract pages

Pages are extracted from the `Report\Layout` file and placed into their own files in the `Report\Pages` subfolder. This makes them easier to manage when inspecting diffs, merging changes and manually editing.

### 7. Order arrays

This step orders the arrays to reduce noise in diffs (we won't see a diff if only the order has changed). This is applied only to arrays where the order does not matter. Currently this is only done to the relationships array in the `DataModelSchema` file.

### 8. Unstuff JSON strings

Some properties contain large json objects stuffed in as strings. In pages, these include `config`, `filters`, `queries` and `dataTransforms` properties. In the `Layout` file, the `config` property also contains "stuffed" json. This step extracts the string into a proper JSON object. This makes it easier to spot changes when examining diffs.

### 9. Consolidate automatic properties

Some report properties change automatically (and often) as the user interacts with PowerBI, namely the `tabOrder` and `z` properties for visual elements. To avoid noisy diffs, this step strips out these properties from all visual elements and adds one array that contains the `tabOrder` for all elements and another array for the `z` value for each visual. That way, all changes related to these two properties are concentrated in these two arrays, rather than being scattered about the file (causing dozens or hundreds of small unimportant diffs).

### 10. Set JSON schema

When extracting files, the tool ads a $schema element to JSON files. This allows editors that have JSON support (e.g. VSCode) to ensure that the json adheres to the rules for each particular file (mandatory properties, allowed properties, validating property values).

In addition to validating the json in the IDE during editing, the files are also validated when packing and any errors are displayed to the user as warnings (the warnings do not prevent packing).

Currently, only the table files are validated. Json schemas for other files can be added fairly easily on an as-needed basis.

## Packing

The pack operation does the opposite of the extract operation. It starts with the repository folder and performs the transformation steps in reverse in order to assemble a new pbit file.

## Interacting with Git

When unpacking a pbit file into a folder, the first step is clearing the folder. All of the contents of the folder are deleted **except** the `.git` folder. Not deleting the `.git` folder allows versioning the files in the repo.

**Important**: all uncomitted changes from a previous unpack operation will be lost when a pbit is unpacked into the folder. The packer tools does not check for uncomitted changes (though it could ask git about this, so this is a possible todo for a future version).

## Editing the source code of a model

VSCode seems to a good choice. It's free, it's fast, it's feature rich, it has GIT integration and it has extensions for editing `M` and `DAX` code.

![VSCode](https://user-images.githubusercontent.com/9460757/150334827-dcf6bc6e-be99-41ee-b78d-775e4a8eeed1.png)

## Workflows

The following describes a typical version-control workflow:

### Initial creation of the repository

1. Save the pbix file as a pbit file
2. Use packer to extract the source code into a folder
3. Initialize a new repository in the created folder
4. Commit the initial version of the source code

### Committing subsequent changes

0. Continue working (making changes) on the pbix model
1. Once a task on the pbix file has been completed, save it as a pbit file
2. Extract the pbit file into the same repository folder
3. Inspect the changes in VSCode and make manual adjustments if needed
4. Commit the changes and supply a descriptive message for the commit

### Going back to a previous version of the model

1. Check out the desired version from source
2. Use packer to assemble a pbit file from it
3. Load the pbit file in PowerBI

### Managing multiple versions of the model (multiple-clients)

1. define a master brach and use it to store all of the functionality that is in common
2. define separate branches for each client (sub-version of the model)
3. when making a change that could be useful across all clients, commit it to the branch for the current client first
4. then switch to the master branch and cherry-pick the change
5. switch to each client branch and merge (or rebase) with the master branch to get the new change

## Good practices

- keep commits small and commit regularly (treat it almost like save game)
- keep commits limited to one topic (one topic can span many commits, but each commit should only be about one topic)
- commits should not break the model (someone in the future should ideally be able to take any commit from the past and assemble a valid pbit file from it)
- learn git so you can sort out the mess whenever you skip the best practices:)


## Requirements

For Packer to work, .NET 6 must be installed on the target machine. If it is not installed, a message will appear in the console with a download link for the .NET 6 runtime (desktop).
