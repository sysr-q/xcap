xcap
====

Simple screen snapping software (Alliterations!), written in C#.

To Use
===
Simply download the __Server.zip__ file, and the __xcap.exe__ file. (These are located in _binaries_)  
Unzip all the contents of Server.zip into the directory you'd like your xcap server to be.  
Run xcap.exe, open Options (right click tray icon, press "Options"), type your server info in, press __Apply__ or __Save__.

Changelog
===

Client
---

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

__2.0.0__
+ Complete rewrite from scratch
+ Handles directories different and isn't hardcoded

__1.0__
+ Official release
+ Spaghetti coded