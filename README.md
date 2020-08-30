# AzureDevOps-BuildRetainer
MSFT was so nice to add a "streamline" feature to the retention settings management for build pipelines in mid-late summer of 2019. As part of this streamlining was the 
(accidental from what I can tell) locking out of project collection administrators from being able to change the retention settings. This resulted in them deleting every 
build that was over 30 days old - even ones that were deployed to production systems. 

This streamlining did not consider or honor existing retention policy settings that had values far larger than 30 days. So the only way to keep build history (and the `$(r:rev)`
mechanic working which was also broken) and protect ourselves from future streamlining is to mark all builds as "retain indefinitely".   

This command line app will take two parameters; your devops URL and a PAT, and will iterate all projects that the PAT can access and set every build to "Retain indefinitely" that 
is not already flagged as such. I set it to run on my workstation as a scheduled task like 3 days a week at 8am. It only takes a minute or three to run and we can have a lot of builds in a few days sometimes.
