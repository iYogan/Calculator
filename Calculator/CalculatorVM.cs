using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Calculator
{
    class CalculatorVM : INotifyPropertyChanged
    {
        private CalculatorImplementer calc;

        public CalculatorVM()
        {
            calc = new CalculatorImplementer();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Получаем занчение введеной формулы
        private String _formula;
        public String Formula
        {
            get { return _formula; }
            set
            {
                _formula = value;
                OnPropertyChanged("ResultRPN"); // уведомление View о том, что изменилась формула и надо обновить поле RPN
                OnPropertyChanged("Result"); // уведомление View о том, что изменилась формула и надо обновить поле результата
            }
        }
        public String ResultRPN { get { return calc.GetRPNFormula(Formula); } }
        public String Result { get { return calc.GetResult(Formula); } }

    }
}
