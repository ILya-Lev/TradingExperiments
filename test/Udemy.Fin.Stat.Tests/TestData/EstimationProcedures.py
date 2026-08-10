"""Python tools for Estimation in Univariate Modelling"""

"""
Copyright (c) 2023 Cameron R. Connell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

import numpy as np
from scipy.special import digamma as psi
from math import sqrt, exp, pi, log, isnan


def psi_prime(x, max=10000):
    if x <= 0:
        print("digamma argument must be > 0")
        return None
    
    terms = x + np.linspace(0, max, max+1)
    reciprocals = 1 / terms**2

    return reciprocals.sum()

def standard(x):
    return 1/sqrt(2*pi) * exp(-x**2/2)

def NormalMixtureLikelihood(x, alpha, mu, sigma):
    size = len(alpha)

    sum = 0.0
    for k in range(size):
        sum += alpha[k] * standard((x - mu[k])/sigma[k]) / sigma[k]
    
    return sum


def ConstrainedEM(data, alpha, mu, sigma, epsilon=0.1, c=0.1, verbose=0, tolerance=0.000001):

    n = len(data)
    size = len(alpha)

    # Initialization
    x = data.data

    alpha_new = alpha
    mu_new = mu
    sigma_new = sigma

    iteration = 0

    if verbose:
        label_string = "|Iter|"

        for k in range(size):
            label = '{:>5}{:>2}|'.format("alpha", k+1)
            label_string += label

        for k in range(size):
            label = '{:>5}{:>2}|'.format("mu", k+1)
            label_string += label

        for k in range(size):
            label = '{:>5}{:>2}|'.format("sigma", k+1)
            label_string += label
    
        print(label_string)
    
        report_string = "|{:>4}|".format(iteration)

        for k in range(size):
            value = '{:>7.4f}|'.format(alpha_new[k])
            report_string += value

        for k in range(size):
            value = '{:>7.4f}|'.format(mu_new[k])
            report_string += value

        for k in range(size):
            value = '{:>7.4f}|'.format(sigma_new[k])
            report_string += value
    
        print(report_string)


    weights = np.ones([size, n])
    normal_pdf = np.ones([size, n])
    variation = np.ones([size, n])

    # Begin EM Iterations
    
    ell0 = 0.0
    for i in range(n):
        try:
            ell0 += log(NormalMixtureLikelihood(data.data[i], alpha, mu, sigma))
        except ValueError:
            continue
    
    new_ell = ell0

    delta = 1
    while delta > tolerance:
        old_ell = new_ell

        iteration += 1
        a = np.ones(size)
        b = np.ones(size)

        alpha_old = alpha_new
        mu_old = mu_new
        sigma_old = sigma_new

        # E-step

        for k in range(size):
            for i in range(n):
                normal_pdf[k, i] = standard((x[i] - mu_old[k])/sigma_old[k]) / sigma_old[k]
        
        for i in range(n):
            pdf = np.dot(alpha_old, normal_pdf[:,i])
            for k in range(size):
                weights[k, i] = alpha_old[k] * normal_pdf[k, i] / pdf
                if isnan(weights[k, i]):
                    raise ValueError("NaN value in weight calculation")

        # M-step

        # Step 2.1
        for k in range(size):
            a[k] = weights[k,].sum()
        
        # Step 2.2
        boolindex = np.ones(size, dtype=bool)
        activeindex = np.ones(size, dtype=bool)

        alpha_iters = 0
        while alpha_iters < 1000:
            numer = 1 - epsilon * (size - activeindex.sum())
            denom = a[activeindex].sum()
            factor = numer/denom

            for k in range(size):
                if activeindex[k]:
                    alpha_new[k] = a[k] * factor
            
            boolindex = alpha_new >= epsilon
            alpha_new[np.logical_not(boolindex)] = epsilon
            alpha_iters += 1

            if all(boolindex):
                break
        
            activeindex = np.logical_and(boolindex, activeindex)

        if alpha_iters >= 1000:
            print("Warning: alpha constraints aborted, too many iterations")
        
        # Step 2.4

        for k in range(size):
            mu_new[k] = np.dot(x, weights[k,]) / a[k]
            
                    
        # Step 2.5
        for k in range(size):
            variation[k,] = (x - mu_new[k]) * (x - mu_new[k])
        
        for k in range(size):
            b[k] = np.dot(variation[k], weights[k,])
        
        # Step 2.6

        sigma_new = np.sqrt(b/a)

        boolindex = sigma_new >= c * np.roll(sigma_new, -1)

        a = np.append(a, a[0])
        b = np.append(b, b[0])

        while not np.all(boolindex):
            k=0
            numer0 = 0
            denom0 = 0
            index0 = [0]
            if not boolindex[k]:
                coeff = c**2
                numer0 = b[k] + coeff * b[k+1]
                denom0 = a[k] + a[k+1]
                index0.append(k+1)
                
                k += 1
                while not boolindex[k]:
                    coeff *= c**2
                    numer0 += coeff*b[k+1]
                    denom0 += a[k+1]
                    index0.append(k+1)
                    k += 1
                    if k == size:
                        break
            
            k += 1                
            while k < size:
                if not boolindex[k]:
                    coeff = c**2
                    numer = b[k] + coeff * b[k+1]
                    denom = a[k] + a[k+1]
                    index = [k, k+1]
                    k += 1

                    if k < size:
                        while not boolindex[k]:
                            coeff *= c**2
                            numer += coeff * b[k+1]
                            denom += a[k+1]
                            index.append(k+1)
                            k += 1
                            if k == size:
                                break

                    if k < size:
                        sigma_new[index[0]] = sqrt(numer/denom)
                        for j in range(1, len(index)):
                            sigma_new[index[j]] = sigma_new[index[j-1]] / c

                k += 1

            if k == size and len(index0) > 1:
                sigma_new[index0[0]] = sqrt(numer0/denom0)
                for j in range(1, len(index0)):
                    sigma_new[index0[j]] = sigma_new[index0[j-1]] / c
            elif k == size + 1:
                index.pop()
                index += index0
                denom += c**2 * coeff * denom0
                numer += numer0
                sigma_new[index[0]] = sqrt(numer/denom)
                for j in range(1, len(index)):
                    sigma_new[index[j]] = sigma_new[index[j-1]] / c

            
            boolindex = sigma_new >= c * np.roll(sigma_new, -1) - 0.000001


        if verbose:
            report_string = "|{:>4}|".format(iteration)

            for k in range(size):
                value = '{:7.4f}|'.format(alpha_new[k])
                report_string += value

            for k in range(size):
                value = '{:7.4f}|'.format(mu_new[k])
                report_string += value

            for k in range(size):
                value = '{:>7.4f}|'.format(sigma_new[k])
                report_string += value
        
            print(report_string)

        new_ell = 0.0
        for i in range(n):
            try:
                new_ell += log(NormalMixtureLikelihood(data.data[i], alpha_new, mu_new, sigma_new))
            except ValueError:
                continue
        
        delta = (new_ell - old_ell)/(new_ell - ell0)


    return [alpha_new, mu_new, sigma_new]


def MLE_StudentT(data, initial=None, verbose=0):
    """
        Function for fitting Student t distribution to data.
        Used EM Algorithm to calculate maximum likelihood estimators
        for the mean or location parameter (mu) the scale factor (rho)
        and the degrees of freedom (nu).

        Parameters
        ----------
        data: Dataset
            The dataset to be fitted.
        initial: list
            A list of initial values for the EM algorithm in the order
                [mu_initial, rho_initial, nu_initial].
            Defaults to method of moment estimators
        verbose: int
            An integer setting the amount of diagnostic information
            to be reported.  Settings are:
                0: No diagnostic information; only final results are reported
                1: Results reported for each EM algorithm iteration

        Returns
        -------
        tuple
            The final MLE estimators returned as a tuple (mu, rho, nu)
    """

    tolerance = 0.0001
    n = len(data)

    x = data.data

    # Initialization
    try:
        if len(initial) == 3:
            mu_new = initial[0]
            rho_new = initial[1]
            nu_new = initial[2]
        else:
            print("Initial parameter list must have length 3")
            print("Reverting to method of moment initial values")

            mu_new = data.mean()

            kurt = data.kurtosis()
            nu_new = 6 / (kurt - 3) + 4

            var = data.variance()
            rho_new = sqrt(var * (nu_new - 2) / nu_new)
    except TypeError:
        mu_new = data.mean()

        kurt = data.kurtosis()
        nu_new = 6 / (kurt - 3) + 4

        var = data.variance()
        rho_new = sqrt(var * (nu_new - 2) / nu_new)

    if verbose:
        iteration=0
        print('{:>10}|{:>20}|{:>20}|{:>20}'.format('Iteration', 'Mu', 'Rho', 'Nu'))
        print('{:>10d}|{:>20}|{:>20}|{:>20}'.format(iteration, mu_new, rho_new, nu_new))

    delta = 1
    while delta > tolerance:

        #E-step
        mu_old = mu_new
        nu_old = nu_new
        rho_old = rho_new

        delta2 = (x - mu_old)**2 / rho_new**2

        w = (nu_old + 1) / (nu_old + delta2)
        q = psi((nu_old+1)/2) - np.log((nu_old + delta2)/2)

        #M-step
        S1 = w.sum()
        S2 = np.dot(w, x)
        mu_new = S2/S1

        v = (mu_new - x)**2
        S3 = np.dot(w, v)
        rho_new = sqrt(S3/n)

        #Root-finding algorithm to update nu (part of the M-step)
        tol_newton = 0.0001
        def F(x):
            sum = np.sum(q - w)
            return log(x/2) + 1 - psi(x/2) + sum/n
        
        def Fprime(x):
            return 1/x - psi_prime(x/2)/2

        nu_new = nu_old - F(nu_old)/Fprime(nu_old)

        while abs(nu_new - nu_old) > tol_newton:
            nu_old = nu_new

            nu_new = nu_old - F(nu_old)/Fprime(nu_old)
        
        if verbose:
            iteration += 1
            print('{:>10d}|{:>20}|{:>20}|{:>20}'.format(iteration, mu_new, rho_new, nu_new))

        delta = abs(mu_new - mu_old) + abs(rho_new - rho_old) + abs(nu_new - nu_old)

    return mu_new, rho_new, nu_new


def EM_NormalMixture(data, size=2, epsilon=0.1, c=0.1, start_length=100, verbose=0, **kwargs):
    """
        Function for fitting normal mixture distribution to data.
        Used EM Algorithm to calculate maximum likelihood estimators
        for the probabilities and mean and standard deviation of the
        component normal distributions.  Employs the constrained
        EM algorithm of Hathaway (J. Statist. Comput. Simul., 1986
        Vol. 23, p. 211-230)

        Parameters
        ----------
        data: Dataset
            The dataset to be fitted.
        size: int
            The number of component normal distributions in the mixture.
            Defaults to 2.
        epsilon: float
            Sets constraint for the probabilities in the constrained
            EM algorithm.  No probability can be less than the constraint.
            Defaults to 0.1
        c: float
            Sets the constraint for the standard deviations of the component
            normal distributions.  The ratio of any 2 standard deviations of
            the components cannot exceed 1/c.  Defaults to 0.1.
        start_length: int
            The number of EM algorithm runs to be executed for the initialization
            process.  Defaults to 100.
        verbose: int
            An integer setting the amount of diagnostic information
            to be reported.  Settings are:
                0: No diagnostic information; only final results are reported.
                1: Each EM run for the initialization process is reported.
                2: Results from each EM iteration is reported for the final run.
                3: Results from each EM iteration is reported for all runs.
        Initialization Parameters:
            If the arguments alpha, mu, and sigma are submitted these are taken as
            initial values for the production EM algorithm run. The initialization
            process is skipped.  alpha, mu, and sigma must be lists of the same
            length, which will be taken as the number of components for the mixture.

        Returns
        -------
        list
            The final MLE estimators are returned as a Python list with the first 3
            components being numpy arrays containing the final estimators for alpha,
            mu, and sigma respectively.  The final entry is the value of the
            log-likelihood value evaluated at the final estimated values.
    """

    n = len(data)

    if "alpha" in kwargs and "mu" in kwargs and "sigma" in kwargs:
        alpha = np.array(kwargs["alpha"], dtype='float64')
        mu = np.array(kwargs["mu"], dtype='float64')
        sigma = np.array(kwargs["sigma"], dtype='float64')
        size = len(alpha)
        if size is not len(mu) or size is not len(sigma):
            print("Initial values must have same length")
            return None
        
        parameters = [[alpha, mu, sigma]]        
        arg = 0
    else:
        mean_range = data.mean()
        std_range = data.stddev()

        alpha = np.ones(size)
        mu = np.ones(size)
        sigma = np.ones(size)

        likelihoods = np.ones(start_length)
        parameters = []
        m = 0
        while m < start_length:
            p = np.ones(size)
            while p.min() < epsilon or p.sum() >= 2 - epsilon:
                for k in range(size-1):
                    p[k] = np.random.uniform()
            p[size - 1] = 2 - p.sum()

            for k in range(size):
                alpha[k] = p[k]
                rand = np.random.uniform()
                mu[k] = rand * mean_range
                rand = np.random.uniform()
                sigma[k] = rand * std_range + 0.1 * std_range
            
            if verbose:
                print("Start iteration ", m+1)

            try:
                results = ConstrainedEM(data, alpha, mu, sigma, epsilon=epsilon, tolerance=0.001, verbose=max(verbose-2, 0))
            except ValueError as error:
                print('Iteration aborted')
                print(error)
                continue

            parameters.append(results)

            sum = 0.0
            for i in range(n):
                try:
                    sum += log(NormalMixtureLikelihood(data.data[i], results[0], results[1], results[2]))
                except ValueError:
                    continue
            
            likelihoods[m] = sum

            m += 1
        
        arg = np.argmax(likelihoods)

    print("Main Run")
    results = ConstrainedEM(data, parameters[arg][0], parameters[arg][1], parameters[arg][2], epsilon=epsilon, verbose=max(verbose-1, 0))
        
    print("Final Results:")
    label_string = ""

    for k in range(size):
        label = '{:>5}{:>2}|'.format("alpha", k+1)
        label_string += label

    for k in range(size):
        label = '{:>5}{:>2}|'.format("mu", k+1)
        label_string += label

    for k in range(size):
        label = '{:>5}{:>2}|'.format("sigma", k+1)
        label_string += label
    
    print(label_string)
    
    report_string = ""

    for k in range(size):
        value = '{:>7.4f}|'.format(results[0][k])
        report_string += value

    for k in range(size):
        value = '{:>7.4f}|'.format(results[1][k])
        report_string += value

    for k in range(size):
        value = '{:>7.4f}|'.format(results[2][k])
        report_string += value
    
    print(report_string)

    sum = 0.0
    for i in range(n):
        try:
            sum += log(NormalMixtureLikelihood(data.data[i], results[0], results[1], results[2]))
        except ValueError:
            print("Warning: log likelihood calculation failed")
            continue
        
    print("Log-likelihood =", sum)

    results.append(sum)
    return results

