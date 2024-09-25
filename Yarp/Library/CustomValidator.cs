using System.ComponentModel.DataAnnotations;

namespace Yarp.Library
{
    public class AllowedMethodsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedMethods = { "GET", "POST", "PUT", "DELETE" ,"PATCH"};

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var methods = value as string[];
            if (methods != null)
            {
                foreach (var method in methods)
                {
                    if (!_allowedMethods.Contains(method.ToUpper()))
                    {
                        return new ValidationResult($"Invalid method: {method}");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
    public class LoadBalancingPolicyAttribute : ValidationAttribute
    {
        private readonly string[] _policies = { "FirstAlphabetical", "RoundRobin", "LeastRequests", "Random", "PowerOfTwoChoices" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var methods = value as string[];
            if (methods != null)
            {
                foreach (var method in methods)
                {
                    if (!_policies.Contains(method.ToUpper()))
                    {
                        return new ValidationResult($"No Policy named : {method}");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
    public class SslProtocolValidatorAttribute : ValidationAttribute
    {
        private readonly string[] _protocols = { System.Security.Authentication.SslProtocols.Tls12.ToString(), System.Security.Authentication.SslProtocols.Tls13.ToString() };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var protocols = value as string[];
            if (protocols != null)
            {
                foreach (var protocol in protocols)
                {
                    if (!_protocols.Contains(protocol))
                    {
                        return new ValidationResult($"No Protocol named : {protocol}");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }

}
