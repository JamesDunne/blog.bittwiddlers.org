---
author: jaymz
categories:
- Uncategorized
date: '2012-11-14T23:04:49'
tags: []
title: XP VMs with VirtualBox
---
For web developers, if you want to test your site on IE7, go download the free XP image from Microsoft <a href="http://www.microsoft.com/en-us/download/details.aspx?id=11575">here</a>. Once it's fully set-up, install IE7 on it; the image comes with the installer on the desktop. Don't bother with the Vista image unless you need to support something OS-specific, which if you do - you should just stop what you're doing and severely rethink your web dev stragedy.

For use with Oracle VirtualBox, you'll have issues with networking, which will prevent you from Activating the VM. Follow these steps to resolve the networking situation:
<ol>
	<li>Download the XP image, obviously: Windows_XP_IE6.exe</li>
	<li>Run the EXE to extract the VHD file (ignore all other files) to somewhere you like</li>
	<li>Fire up VirtualBox</li>
	<li>Create a new virtual machine using the existing VHD file you just extracted, obvious settings apply</li>
	<li>Go download the Intel PRO driver at http://downloadmirror.intel.com/8659/eng/PRO2KXP.exe</li>
	<li>Place that EXE into a new ISO image using whatever ISO tools you wish (cygwin has mkisofs)</li>
	<li>Mount the ISO you created on FIRST boot of the VM and install the driver as immediately as you can. This will help you be able to Activate the VM over the Internet.</li>
	<li>Open the mounted ISO from within the VM and run the driver EXE installer.</li>
	<li>Reboot should be safe at this point.</li>
</ol>
NOTE: If you don't Activate after the second boot, your VM is hosed and you have to start from scratch again (just run the XP EXE and replace the VHD file). I did this at least 4 times to try to find the right procedure.

After you finally activate your VM, you should be fine to install IE7. Don't bother doing that before otherwise you'll just waste your time because the VM won't let you log in after three boots without being activated.

Now you'll probably want some sort of decent JavaScript debugger. Well, I've got some good news and some bad.

The good news is you can get a basic JavaScript stack trace when an exception is thrown but only if you install <strong>Microsoft Script Debugger</strong>. The bad news is that this tool flat-out sucks and you don't have many other options. If you know of some, please let me know.