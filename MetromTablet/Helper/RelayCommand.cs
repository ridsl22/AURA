using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetromTablet
{
	public class RelayCommand : ICommand
	{
		#region Declarations

		readonly Func<Boolean> _canExecute;
		readonly Action _execute;
		readonly Predicate<object> _canExecuteOverloaded;
		readonly Action<object> _executeOverloaded;
		private bool _overloaded;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand&lt;T&gt;"/> class and the command can always be executed.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{

			if (execute == null)
				throw new ArgumentNullException("execute");
			_executeOverloaded = execute;
			_canExecuteOverloaded = canExecute;
			_overloaded = true;
		}

		public RelayCommand(Action execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action execute, Func<Boolean> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			_execute = execute;
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		public event EventHandler CanExecuteChanged
		{
			add
			{

				if (_canExecute != null || _canExecuteOverloaded != null)
					CommandManager.RequerySuggested += value;
			}
			remove
			{

				if (_canExecute != null || _canExecuteOverloaded != null)
					CommandManager.RequerySuggested -= value;
			}
		}

		public Boolean CanExecute(Object parameter)
		{
			if (_overloaded)
				return _canExecuteOverloaded == null ? true : _canExecuteOverloaded(parameter);
			else
				return _canExecute == null ? true : _canExecute();
		}

		public void Execute(Object parameter)
		{
			if (_overloaded)
				_executeOverloaded(parameter);
			else
				_execute();
		}

		#endregion
	}
}
