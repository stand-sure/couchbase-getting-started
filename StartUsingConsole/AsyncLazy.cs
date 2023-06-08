namespace StartUsingConsole;

using System.Runtime.CompilerServices;

//// inspired by Stephen Toub's  https://devblogs.microsoft.com/pfxteam/asynclazyt/

/// <summary>
///     Provides support for async lazy initialization.
/// </summary>
/// <typeparam name="T">Specifies the type of element being lazily initialized.</typeparam>
/// <remarks>
///     Use cases:
///     <list type="bullet">
///         <item>The non-async factory is slow and awaiting it is desirable.</item>
///         <item></item>
///     </list>
/// </remarks>
public class AsyncLazy<T> : Lazy<Task<T>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncLazy{T}" /> class
    ///     that uses a specified initialization function.
    /// </summary>
    /// <param name="valueFactory">
    ///     The <see cref="Func{T}" /> invoked
    ///     to produce the lazily-initialized value when it is needed.
    /// </param>
    public AsyncLazy(Func<T> valueFactory)
        : base(() => Task.Factory.StartNew(valueFactory))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncLazy{T}" /> class
    ///     that uses a specified task initialization function.
    /// </summary>
    /// <param name="taskFactory">
    ///     The <see cref="Func{U}" /> invoked to produce a lazily-initialized <see cref="Task{T}" /> when it is needed.
    /// </param>
    public AsyncLazy(Func<Task<T>> taskFactory)
        : base(() => Task.Factory.StartNew(taskFactory).Unwrap())
    {
    }

    /// <summary>
    ///     Allows <code>await foo</code> in lieu of <code>await foo.Value</code>.
    /// </summary>
    /// <returns>A <see cref="TaskAwaiter{T}" />.</returns>
    public TaskAwaiter<T> GetAwaiter()
    {
        return this.Value.GetAwaiter();
    }

    /*
     * this.Value is Task<T>
     */

    /// <summary>
    ///     Gets the value.
    /// </summary>
    /// <returns>
    ///     The <see cref="T" />.
    /// </returns>
    /// <remarks>
    ///     This method is intended for scenarios
    ///     where <code>await foo</code> is not viable
    ///     (e.g. in properties).
    /// </remarks>
    public T GetValue()
    {
        return this.GetAwaiter().GetResult();
    }
}