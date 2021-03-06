namespace NServiceBus.ObjectBuilder.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.ObjectBuilder2;
    using Microsoft.Practices.Unity;

    public class PropertyInjectionBuilderStrategy : BuilderStrategy
    {
        IUnityContainer unityContainer;
        static IDictionary<Type, List<Tuple<string, object>>> configuredProperties = new Dictionary<Type, List<Tuple<string, object>>>();
 
        public PropertyInjectionBuilderStrategy(IUnityContainer unityContainer)
        {
            this.unityContainer = unityContainer;
        }

        public static void SetPropertyValue(Type type, string name,object value)
        {
            List<Tuple<string, object>> properties;
            if (!configuredProperties.TryGetValue(type, out properties))
            {
                configuredProperties[type] = properties = new List<Tuple<string, object>>();
            }

            properties.Add(new Tuple<string, object>(name, value));
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            var type = context.BuildKey.Type;
            if (!type.FullName.StartsWith("Microsoft.Practices"))
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (!property.CanWrite)
                    {
                        continue;
                    }

                    if (unityContainer.IsRegistered(property.PropertyType))
                    {
                        property.SetValue(context.Existing, unityContainer.Resolve(property.PropertyType),null);
                    }

                    List<Tuple<string, object>> configuredProperty;
                    if(configuredProperties.TryGetValue(type, out configuredProperty))
                    {
                        var p = configuredProperty.FirstOrDefault(t => t.Item1 == property.Name);

                        if (p != null)
                        {
                            property.SetValue(context.Existing, p.Item2,null);
                        }
                    }
                }
            }
        }
    }
}