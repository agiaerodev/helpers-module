
namespace Ihelpers.Helpers.Interfaces
{
    /// <summary>
    /// Represents a class helper interface for a type of EntityBase.
    /// </summary>
    /// <typeparam name="TEntity">The type of EntityBase.</typeparam>
    public interface IClassHelper<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets or sets the delimiter to be used in the CSV file.
        /// </summary>
        string delimiter { get; set; }

        /// <summary>
        /// Converts a single EntityBase object to a CSV file stream.
        /// </summary>
        /// <param name="input">The EntityBase object to be converted to a CSV file stream.</param>
        /// <returns>A stream of the CSV file.</returns>
        Stream toCsvFile(TEntity input);

        /// <summary>
        /// Converts a list of EntityBase objects to a CSV file stream.
        /// </summary>
        /// <param name="input">The list of EntityBase objects to be converted to a CSV file stream.</param>
        /// <returns>A stream of the CSV file.</returns>
        Stream toCsvFile(List<TEntity> input);

        /// <summary>
        /// Converts a list of EntityBase objects to a CSV file stream with specified fields and headings.
        /// </summary>
        /// <param name="input">The list of EntityBase objects to be converted to a CSV file stream.</param>
        /// <param name="fields">The fields to be included in the CSV file.</param>
        /// <param name="headings">The headings to be used in the CSV file for each field.</param>
        /// <returns>A stream of the CSV file.</returns>
        Stream toCsvFile(List<TEntity> input, List<string>? fields, List<string>? headings);

        /// <summary>
        /// Generates a stream from a string.
        /// </summary>
        /// <param name="s">The string to be converted to a stream.</param>
        /// <returns>A stream of the string.</returns>
        Stream GenerateStreamFromString(string s);
    }
}