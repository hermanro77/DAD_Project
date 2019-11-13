# DAD_Project
Meeting rooms
## Questions
1) How does the server_URL looks like? Does it contain the portnumber? The identifier?
2) Does the same apply for client_URL?
3) How does the script look like?
4) How does servers know about each other?
5) When a client ask for something from the server, can the method just return the requested information, or does the servar has to
call the client with its interface methods?
\t if so, also change in commontypes
6) Is the lock used in Server to lock clients list correct?


### Notes
List is not serializable, use Array.
Use gossip when distributing meeetings
When client asks for available meetings (s)he should only get status on the meetings the client already know
