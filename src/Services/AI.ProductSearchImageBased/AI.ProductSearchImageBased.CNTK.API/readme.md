# CNTK + IIS

IIS Express by default has a stack limit of 256K, but in order to evaluate deep neural network, we need to increase the stack limit.
Execute following command:

```
editbin /stack:1048576 "C:\Program Files\IIS Express\iisexpress.exe"
```
