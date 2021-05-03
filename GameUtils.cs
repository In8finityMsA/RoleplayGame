using System;

namespace KashTaskWPF
{
    public static class GameUtils
    {
        public static T[] SubArray<T>(T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        public static object GetObjectFromString(string className, string[] parameters)
        {
            Type type = Type.GetType(className);
            if (type == null)
            {
                throw new ArgumentException($"Type with specified name wasn't found - {className}.");
            }

            object createdObject = null;
            try
            {
                if (parameters.Length > 0)
                {
                    createdObject = Activator.CreateInstance(type, new object[] {Int32.Parse(parameters[0])});
                }
                else
                {
                    createdObject = Activator.CreateInstance(type);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException($"Cannot construct specified type - {className}. " +
                                            $"Parameters: {parameters}.");
            }

            return createdObject;
        }
    }
}