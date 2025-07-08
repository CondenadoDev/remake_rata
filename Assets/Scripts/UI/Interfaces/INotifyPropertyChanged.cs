using System;

public interface INotifyPropertyChanged
{
    event PropertyChangedEventHandler PropertyChanged;
}

public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);

public class PropertyChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
        
    public PropertyChangedEventArgs(string propertyName)
    {
        PropertyName = propertyName;
    }
}