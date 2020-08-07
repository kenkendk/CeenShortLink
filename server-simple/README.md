This folder contains a bare-bones short-link service.

It requires simply that the link file is updated with redirect instructions. The file is continously monitored and will be re-read when changed.

The code is contained in a single file with less than 300 lines including blanks and comments. The actual code is less than 200 lines, where the majority is for handling changed file monitoring. 

Writing and testing the code took appx. 1 hour.

This example is a contrast to the larger short-link service that includes admin, monitoring and logging features. If you have a setup where logging and administration is performed elsewhere, having a lean service such as this could be preferable.