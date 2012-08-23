xcap
====

Simple screen snapping software (Alliterations!), written in C#.

To Use
===

Client
---
+ Download the latest version of the client binary.
+ The binaries are uploaded in the style of __xcap-X.Y.Z.exe__ _(Download the binary with the highest numbers)_
    + __X__ is your major build.
    + __Y__ is your minor build.
    + __Z__ is your patch number.
+ Run the binary.
+ _Optional:_ Create a shortcut to the _xcap-X.Y.Z.exe_ file in your startup folder, to have xcap start on load.
+ Open up __Options__

Server
---
+ Download the latest server package.
+ Server binaries are uploaded in the style of __Server-X.Y.Z.zip__ _(Download the binary with the highest numbers)_
    + __X__ is your major build.
    + __Y__ is your minor build.
    + __Z__ is your patch number.
+ Upload the zip to your server via FTP, file manager or other, and unpack it.
    + _or:_ Unpack the zip and upload the files individually
+ Test that your server is valid by opening xcap, going into options, typing in the URL and pressing __Apply__

Simply download the __Server.zip__ file, and the __xcap.exe__ file. (These are located in _binaries_) 
Unzip all the contents of Server.zip into the directory you'd like your xcap server to be.  
Run xcap.exe, open Options (right click tray icon, press "Options"), type your server info in, press __Apply__ or __Save__.

Changelog
===

Client
---

__2.2.0__
+ Fixed some small issues with the icons, again.
+ Made client now work with _2.1.0_ or higher servers.
+ Cleaned up the splash image, so it doesn't look as bad on non-white backgrounds.
+ Misc changes to the request for uploading.

__2.1.1__
+ Fixed some small things regarding the icon percentages to hopefully fix "A generic error occurred in GDI+" crashes
+ Went through and made sure *all* objects are disposed of correctly, hopefully lowering memory usage

__2.1.0__
+ More bug fixes
+ Changed various things in the options menu
+ You can only have one instance of xcap open, thanks to mutex
+ Lovely splash screen so you can tell when xcap opens

__2.0.0__
+ Complete rewrite of client
+ Options menu
+ Various bug fixes
+ New form handling

__1.0__
+ Official release
+ Ridden with bugs

Server
---

__2.1.0__
+ Packed most scripts into one.
+ Modified uploading so it won't actually allow non-valid versions.
    + _Previously it would allow uploads, even from a non-valid version._
+ __This will require a client upgrade to 2.2.0 or higher.__

__2.0.0__
+ Complete rewrite from scratch
+ Handles directories different and isn't hardcoded

__1.0__
+ Official release
+ Spaghetti coded

Todo
===

Client
---
+ Fix occasional dead pixels appearing in snap.
    + _Cause: Unknown at the moment._
+ Fix errors messing up the icon.
+ Create server testing feature, to find problematic server setups.
    + Probably just a launch param like `xcap-X.Y.Z.exe /test <SERVER-ROOT>`

Server
---
+ Decent version checking.
    + _Allow things such as __X.Y+.*__, or simply allow anything above X.Y through._
+ Allow easy changing of page layout through templates.
    + _Will probably require extra files, so time will tell._