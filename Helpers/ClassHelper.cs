using ChoETL;
using Ihelpers.Helpers.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Ihelpers.Helpers
{
    public class ClassHelper<TEntity> : IClassHelper<TEntity> where TEntity : class
    {
        public string delimiter { get; set; } = ",";

        #region Obsolete
        public Stream toCsvFile(TEntity input)
        {
            return toCsvFile(input, true);

        }
        public Stream toCsvFile(List<TEntity> inputs)
        {
            return toCsvFile(inputs, true);

        }
        [Obsolete]
        public Stream toCsvFile(List<TEntity> input, List<string>? fields, List<string>? headings)
        {
            Stream output = new MemoryStream();

            List<string> csvList = new List<string>();

            foreach (dynamic Tentity in input)
            {
                Tentity.InitializeForCSV();
                //Messy library that doesnt generate newline so 1 aditional call is needed to get headers
                csvList.AddRange(toCsvStringBuilder(Tentity, input.IndexOf(Tentity) == 0).ToString().Split("\r\n").ToList());


            }


            return RemoveColumnByIndex(csvList, fields, headings);

        }

        [Obsolete]

        private Stream toCsvFile(object? input, bool withHeaders = true)
        {
            Stream output = new MemoryStream();
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var serializedObject = JsonConvert.SerializeObject(input, jsonSerializerSettings);


            using (var jsonResult = ChoJSONReader.LoadText(serializedObject))
            {
                if (withHeaders)
                {
                    using (var writer = new ChoCSVWriter(output)
                     .WithFirstLineHeader()
                    .WithDelimiter(delimiter))
                    {
                        writer.Write(jsonResult);
                    }
                }

                output.Position = 0;

                return output;
            }
        }
        public Stream RemoveColumnByIndex(List<string> csvStrings, List<string>? fields, List<string>? headings)
        {
            List<string> lines = new List<string>();

            Stream outputStream = new MemoryStream();


            //fields = field1, field3, field5
            List<int> allowedIndexes = new(); //0,2

            for (int i = 0; i < csvStrings.Count; i++)
            {

                //Get the valid column indexes

                var line = csvStrings[i];


                if (i == 0)
                {

                    var allHeaders = line.Split(delimiter).ToList(); //field1,field2,field3

                    foreach (var header in allHeaders)
                    {
                        if (fields.Contains(header))
                        {
                            allowedIndexes.Add(allHeaders.IndexOf(header));
                        }
                    }

                }

                //remove invalid colum indexes 
                List<string> values = new List<string>();




                values.Clear();

                if (i == 0) // headings = "id", "Fecha de Creación", "Fecha Ultima Actualización"
                {
                    foreach (var field in fields)
                    {
                        line = line.Replace(field, headings[fields.IndexOf(field)]);
                    }

                }
                var cols = line.Split(delimiter).ToList(); //id, created_at, updated_at

                //Agregar solo los index que coincidan con los allowed indexes

                foreach (var index in allowedIndexes)
                {
                    values.Add(cols[index]);
                }
                //var www = cols.Where(col => allowedIndexes.Contains(col.IndexOf(col))).ToList();

                //values.AddRange(cols.Where(Col => allowedIndexes.Contains(cols.IndexOf(Col))).ToList());



                var newLine = string.Join(delimiter, values);

                lines.Add(newLine);

            }
            using (StreamWriter writer = new StreamWriter(outputStream, leaveOpen: true))
            {
                foreach (var lineIn in lines)
                {
                    writer.WriteLine(lineIn);
                }
            }

            return outputStream;


        }
        private StringBuilder toCsvStringBuilder(TEntity? input, bool withHeaders = true)
        {
            StringBuilder output = new StringBuilder();

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var serializedObject = JsonConvert.SerializeObject(input, jsonSerializerSettings);
            // JObject serializedObject = (JObject)JToken.FromObject(input);

            serializedObject = serializedObject.Replace("_", "__");

            using (var jsonResult = ChoJSONReader.LoadText(serializedObject.ToString()))
            {
                if (withHeaders == true)
                {
                    using (var writer = new ChoCSVWriter(output)
                    .WithFirstLineHeader(withHeaders)

                    .WithDelimiter(delimiter))
                    {
                        writer.Write(jsonResult);
                    }
                }
                else
                {
                    using (var writer = new ChoCSVWriter(output)
                   .WithDelimiter(delimiter))
                    {
                        writer.Write(jsonResult);
                    }
                }


                return output;
            }
        }

        #endregion


        public Stream GenerateStreamFromString(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }
        public static Stream GenerateStreamFromStr(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }



    }


    public static class ClassHelper
    {

        /// <summary>
        /// Gets the value of property that belongs to an unknow object by the property name
        /// </summary>
        /// <param name="obj">The unknow object</param>
        /// <param name="propertyName">the property name</param>
        /// <returns></returns>
        static public async Task<dynamic> GetValObjDy(this object obj, string propertyName)
        {
            try
            {
                //if the property name is valid them get the value and return it
                dynamic value = obj.GetType().GetProperty(propertyName).GetValue(obj, null);

                return value;
            }
            catch
            {
                //if something is wrong with the property name return null
                return null;
            }

        }
    }
}
