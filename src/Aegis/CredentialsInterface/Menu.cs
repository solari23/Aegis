using Aegis.Core;

namespace Aegis.CredentialsInterface;

/// <summary>
/// A menu that displays options to the user and gets their selection.
/// </summary>
/// <typeparam name="TIdentifier">The type of the options' unique identifiers.</typeparam>
public class Menu<TIdentifier>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Menu"/> class.
    /// </summary>
    /// <param name="ioStreamSet">The IO streams.</param>
    /// <param name="prompt">The prompt to display at the top of the menu.</param>
    /// <param name="options">The options to present.</param>
    public Menu(IOStreamSet ioStreamSet, string prompt, params Option[] options)
    {
        ArgCheck.NotNull(ioStreamSet, nameof(ioStreamSet));
        ArgCheck.NotEmpty(prompt, nameof(prompt));
        ArgCheck.NotEmpty(options, nameof(options));

        this.IO = ioStreamSet;
        this.Prompt = prompt;
        this.Options = options;
    }

    /// <summary>
    /// Gets the IO streams.
    /// </summary>
    private IOStreamSet IO { get; }

    /// <summary>
    /// Gets the prompt to display at the top of the menu.
    /// </summary>
    private string Prompt { get; }

    /// <summary>
    /// Gets the options to present.
    /// </summary>
    private Option[] Options { get; }

    /// <summary>
    /// Displays the menu to the user and returns the user's selection.
    /// </summary>
    /// <returns>The identifier of the selection menu option.</returns>
    public TIdentifier GetSelection()
    {
        // Special case.
        if (this.Options.Length == 1)
        {
            return this.Options[0].Id;
        }

        this.IO.Out.WriteLine(this.Prompt);

        for (int i = 0; i < this.Options.Length; i++)
        {
            this.IO.Out.WriteLine($"[{i + 1}] {this.Options[i].DisplayString}");
        }

        var inputPrompt = new InputPrompt(
            this.IO,
            $"Enter selection [1-{this.Options.Length}]: ",
            validator: s => int.TryParse(s, out var v) && v >= 1 && v <= this.Options.Length);

        var input = inputPrompt.GetValue();
        var selectedIndex = int.Parse(input);

        return this.Options[selectedIndex].Id;
    }

    /// <summary>
    /// Data structure representing a menu option.
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class.
        /// </summary>
        /// <param name="id">The identifier for the option.</param>
        /// <param name="displayString">The string to display for the option.</param>
        public Option(TIdentifier id, string displayString)
        {
            this.Id = id;
            this.DisplayString = displayString;
        }

        /// <summary>
        /// Gets the identifier for the option.
        /// </summary>
        public TIdentifier Id { get; }

        /// <summary>
        /// Gets the string to display for the option.
        /// </summary>
        public string DisplayString { get; }
    }
}
