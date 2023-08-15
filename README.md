# Folder-Backup
## Structure & execution
### Repository structure
Since this coding task is for a QA position, I decided to show some of the QA I did while working on it. The actual project lives in Folder-Backup, the testing I did in Folder-Backup-Test. 
## Notable decisions
### No hashing
I decided to not hash the contents of my files since I believe it would make the process slower overall. To hash a file I would need to fully read it, but since hash-collision is always possible, I would still need to read the contents of two files to make sure they are actually the same. So, for the most common case, the case that the file stayed the same, hashing adds one more read of a file to the overall workload.   
### No FileSystemWatcher
During a first preliminary research about different ways of implementing a backup system, I found multiple posts of people complaining that the FileSystemWatcher was unreliable. There seemed to be credible sources stating that the FileSystemWatcher misses changes or does not remember them if its buffers are full. Sine a backup system needs to be reliable before all I decided not to use the FileSystemWatcher for my implmentation.  
## Notable problems
### MemoryMappedFiles & chunk size
My testing showed that the StreamReader only supports file sizes up to 2GB, to support bigger file sizes I needed to switch to MemoryMappedFiles. That posed the qustion of how big the file-chunks are that the program should read at a time. I did some basic profiling for file sizes of 2GB. If I read in the contents of two files one byte at a time to compare them bytewise, it took about 7 minutes. The most efficient chunk size in my testing seemed to be 4096 bytes, for an average runtime of 45 seconds.    
### Testing for FileAccessRights
I decided I wanted to have a robust parser for command line arguments that throws errors when anything is amiss. One problem I wanted to catch were missing access rights for the folders the program would work on. When I tested the program on a linux machine I realized that my implementation with FileAccessRights would only work on windows machines and that even just checking for the correct access rights on windows is not as straight forward as I first thought. I then decided to instead of trying to catch a possible error on start-up to implement a more robust error handling during runtime.    
