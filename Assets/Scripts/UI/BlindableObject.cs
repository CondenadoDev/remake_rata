// BindableObject.cs - Base class for bindable data
using UnityEngine;

/// <summary>
/// Simple ScriptableObject implementing INotifyPropertyChanged so that data
/// objects can be used with the existing binding system.
/// </summary>
public abstract class BindableObject : ScriptableObject, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T field, T value, string propertyName)
    {
        if (!Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}