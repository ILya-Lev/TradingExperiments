EstimationProcedures module, containing Python functions for fitting models to financial data.  Currently this module contains the procedures

MLE_StudentT
EM_NormalMixture
We give basic documentation for the use of these procedures.

The function MLE_StudentT calculates the maximum likelihood estimators for the parameters of a Student t distribution including location and scale parameters as well as the degrees of freedom.  The signature of the function is

MLE_StudentT(data, initial, verbose)
The first argument data is a Dataset object containing the data to be fitted to a Student t distribution.   initial is a list of length 3 providing initial values for the mean (mu), scale (rho) and degree of freedom (nu) parameters, which will be taken in the form

[mu, rho, nu]
If this argument is not provided, the function uses the method of moment values as the starting values for the EM algorithm. verbose is an integer setting the amount of diagnostic information that will be output for the user.  Setting verbose=0 (the default) will produce no output, only the final estimated values will be returned.  With verbose=1 the updated values of the estimators will be displayed for each iteration of the EM algorithm.  The return value of MLE_StudentT is a tuple consisting of the final estimated values for mu, rho, and nu.

If dataset is the Dataset object with the data to be fitted to a generalized Student t distribution, the function call

parameters = MLE_StudentT(dataset)
will calculate the method of moments estimators of the parameters of the Student t for the given dataset, and return the final estimate to the tuple parameters in the form

(mu, rho, nu)

If the user wishes to provide starting values of the parameters for the EM algorithm, submit the those values in the form of a list start and call the function as

parameters = MLE_StudentT(dataset, initial=start)
EM_NormalMixture fits a normal mixture distribution to data, using a constrained version of the EM algorithm.   The algorithm is described in R. J. Hathaway, "A Constrained EM Algorithm for Univariate Normal Mixtures", J. Statist. Comput. Simul. 1986, Vol. 23, p 211-230

The signature for the function is

EM_NormalMixture(data, size, epsilon, c, start_length, verbose, **kwargs)
data is the Dataset object containing the data to be fitted.  size sets the number of component normal distributions in the mixture (default is 2).  epsilon sets the constraint for the probability parameters (defaults to 0.1).  c sets the constraint for the ration of standard deviations (defaults to 0.1).  start_length sets the number of runs of the EM algorithm for the initialization process (defaults to 100).  verbose is an integer setting the amount of diagnostic information that will be displayed to the user.  The available settings are as follows:

0: No diagnostic information; only final results are reported (default).

1: Each EM run for the initialization process is reported.

2: Results from each EM iteration is reported for the final run.

3: Results from each EM iteration is reported for all runs.

Additional keyword arguments may be provided to submit starting values for the algorithm.  If submitted, the initialization process will be omitted.  These initial values must take the form of 3 Python lists, all of them the same length, and must be named alpha, mu, and sigma.  They should contain the initial values for the named parameters, and are assumed to be in a consistent order, so that first component of alpha is taken as the probability associated with a component normal distribution with mean the first component of mu and standard deviation the first component of sigma.  Note that alpha corresponds to the probability parameters for the mixture.

The return value of EM_NormalMixture is a Python list with 3 components.  The first 3 components are Numpy arrays containing the final maximum likelihood estimates.  The first array contains the alpha estimates, the second array contains the estimates for mu and the third contains the estimates for sigma.  The 4th component of the return list contains the final value of the log-likelihood function.

The function may be invoked with the simple statement

parameters = EM_NormalMixture(dataset)
where dataset is the Dataset object containing the data desired to be fitted.