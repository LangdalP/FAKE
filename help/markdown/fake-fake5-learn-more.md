# FAKE 5 - Learn more

> This is especially about the FAKE 5 update, [here you can learn about FAKE](fake-what-is-fake.html)

FAKE 5 is a huge undertaking in modularizing and porting FAKE 4 to dotnet-core.
At the same time we want to use that opportunity to pay old API debts.

On top of that compatibility was always a major concern when updating FAKE,
therefore we need a clear [migration path](fake-migrate-to-fake-5.html) for existing projects.

So before I go into the details of what changed and what the vision is going forward let me say:

> Every help matters. I've tried - but the code-base is just too large to handle.
> I think we now have a good foundation to finish the remaining work.
> Therefore I decided to make the finish line of FAKE 5 a community effort.
> If your favorite module is missing, please port it and send a PR (see Open Work section below).

## How stable is it? What is RC? Should I use it?

First, let me say that there are two distributions of FAKE 5.

* The first one is an update to the 'regular' FAKE 4 package on NuGet.
  It's very safe to update to this package as mentioned in the [migrate to fake 5 guide](fake-migrate-to-fake-5.html).
  Update the package and fix the `Obsolete` warnings to use the new API. There might still be some minor issues in the new API but it is considered stable now (ie no more breaking changes).
* The second one is the new dotnet-core version of FAKE.
  This version is brand-new, was migrated to a new platform and has several API changes.
  We call it the "new runner" it bundles the support for the `#r "paket: ..."` references and has a new [CLI](fake-commandline.html). At this point we no longer break the API but there might still be some minor issues.

> Fake 5 and this website are already released with Fake 5, so it might be more stable than you think.

## Modules? Packages? Paket?

In the past it was cumbersome and hard to extend your own builds with customized logic or share logic across builds easily.
Paket loading scripts helped a lot in that issue space, but now you have quite a lot infrastructure for simply running a build script.
So in a sense it made sense to combine all good parts of paket, dotnet-core and the huge fake library to go to the next level. The result is a standalone, dependency free version of Fake which is modular and extendible. On top of that you can now use all the Fake 5 modules in your "regular" F# projects!

You only need to know the simple [paket.dependencies](http://fsprojects.github.io/Paket/dependencies-file.html) syntax (basically a list of nuget packages) and are able to add [custom logic](fake-fake5-custom-modules.html) to your script.
If you are in the scripting area there is not even a need to save `paket.dependencies` as a separate file. Just put it on top of the script. Fake will take care of the rest.

## Open Work / Help wanted

This section describes work that still need to be done to finally release FAKE 5.

* Porting modules (See Modules -> Legacy), please read the migrate modules section in the [contribution guide](contributing.html).
* Missing Obsolete Attributes in the old API (please help when you migrate and notice anything)
* Bootstrapping via Script and System to update scripts automatically + lock FAKE version
* Concept and work on how we want do document modules and module overviews (website is generated automatically)
* Missing documentation
* Update documentation
* Fixing issues with the new website
* Implement a special formatting for the obsolete warnings -> parse them -> suggest new API dependencies file.

## New API-Design

The old API suffered from several issues:

* `AutoOpen` polluted the "Fake" namespace.

  One problem with this that some modules introduced generic function names which are perfectly valid for other modules as well. Now the second module needs to introduce a "quirk" name.

* No visible structure and unified naming made it incredible hard to find an API.

For example consider the old API `CreateDirectory` which is a globally available.
It needs to have its context "Directory" in the name, but this means longer method names.
Imagine `Directory.Create` it's not really that much longer - but the IDE can support me a lot more.
This old API design lead to ambiguities and even user issues.

Therefore we propose a new [API design in the contributors guide](contributing.html)
