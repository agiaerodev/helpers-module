using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Helpers
{
    public static class IConvert
    {

        public static bool ToBooleanFromExpression([NotNullWhen(true)] object? value)
        {

            try
            {
                //Obtaining the correct string format
                string plainString = value.ToString().Replace("&&", "And").Replace("||", "Or").Replace("\"", "'").Replace("==", "=");

                DataTable systemAnalizer = new DataTable();

                bool judmentResult = Convert.ToBoolean(systemAnalizer.Compute(plainString, null));

                return judmentResult;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}
