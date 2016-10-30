# A selection of code scraps.

This repository is so I can store and share a selection of code pieces.

## Overview
The repository contains a number of small dotnet source projects, each too small to
warrant its own project.

I also wanted to be able to build core project assemblies, without unneeded 
dependencies. By breaking code into source project's I can create a core project
that only takes the pieces of interest.

My code projects use lower-case names to distinguish them from normal Camel case assembly projects.

## How to use

1. Clone the repository to a local directory.
2. Add / Update your _global.json_ to include the local repo folder.
3. Add source only project references to you *project.json* 

For example, look at the __sln__ folder, I use it to create concreate assemblies
by combining different _source projects_

## See also

1. Another source-only project: 
    [shaynevanasperen/Quarks](https://github.com/shaynevanasperen/Quarks) 

## Scraps
Many of my scraps revolve around extension / helpers / adapters for 
particular libraries. Hence, scraps are named after the 'source' or
'input' library they use. 

Following is a brief overview of each scrap.
 
### kwd.keepass
Provides a secure configuration store (_IConfigurationSource_) in dotnet.
To keep things simple, this is very opinionated about how configuration is stored.

1. The filename defaults to the Executing assembly name with __.kdbx__.

2. The key file used to access the Db has the same file name, but with extension __.key__.

2. Path search.
 2.1 **%HOMEPATH%.keepass** is searched first.
 2.2 Failing that CWD searched.  
 2.3 Failing that every parent directory of current is searched.
First found is used.

3. If no db found, an new one is created in CWD. 

4. A configuration Section corresponds to a Keepass group.

The namespace of a requested object reflects the configureation section.
e.G for a class **MyApp.Services.Config.AppOptions**
The _Keepass Db__ will use a group **/MyApp/Services/Config** inside
the group will be an entry **AppOptions**, the values in that entry 
will reflect the config values served up.


