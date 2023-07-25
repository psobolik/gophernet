# Things to do (maybe)
* Allow for seleting Light or Dark theme. (Persist choice.)
* Allow choosing font size.
* Disable Save as... appropriately
 
# Avalonia 
It looks like there is a bug in the way the Avalonia TextBlock handles leading 
spaces when it is using a fixed width font, which manifests itself when rendering 
ASCII art.

# Git
This shouldn't really be in one big solution. It would be better to make three different
solutions in three different repositories, with the a reference to the library repository
in the client repositories.
