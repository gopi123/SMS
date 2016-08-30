using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.CustomValidation
{
    public class RequiredArrayAttribute : ValidationAttribute,IClientValidatable
    {
        public override bool IsValid(object value)
        {
            var list = value as IList<int>;
            if (list != null)
            {
                return list.Count > 0;
            }
            return false;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "emptyarray",
                ErrorMessage = "Please select centre code"
            };            
            yield return rule;
        }
    }
}