This project is an Achron Matchmaker Server emulator.

A while ago, the site achrongame.com went offline.
That was a very sad thing, and left achron the time-traveling-rts entirely unable to be played!

This is a server emulator that allows games to be created.

Current Status:
The server emulator works - you can create and join games.
Games never display the correct number of players in the games this is cosmetic only, and games function as expected.
Also, as far as we are aware UPnP does not work; and so all users should forward ports 7014, 7013, and 7614.

How to use the server:
1) Download the source code, and build it using Visual Studio. (or download one of the packages from the release page)

2) Alter the included file named hosts and replace 1.1.1.1 with the ip address of the server OR compile the achron patcher and patch the client to use the IP of the server.

3) each player will need to replace their hosts file (C:\Windows\System32\drivers\etc\hosts) with this new altered host file, or patch their client.

3(a)) once the hosts file is replaced, make sure to run the following command in cmd.exe as admin: 
ipconfig /flushdns
this will apply the new hosts file. Alternatively, once the hosts file is replaced users may restart thier computers. 
This step is not required for a patched client.

4) the player hosting the server should ensure port 80 is forwarded, in addition to the other ports mentioned before.

5) run the server.

6) players may now run achron, and create a game, and join that game.

7) once the game has been finished, ideally the server should be restarted if more games are required.

UPDATE:
Games now display the correct number of players, correct number of max players, show when games are in progress, removes games that are finished, and removes games when the host leaves. However these features are only avaliable if you build the server from source; the release version does not include these features.
