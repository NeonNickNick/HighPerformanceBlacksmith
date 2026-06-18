namespace BlacksmithCore.Infra.Enum
{
    public static class BlacksmithEnumRegistry
    {
        private static readonly EnumRegistryInstance _registry = new();

        public static IReadOnlyDictionary<Type, IBlacksmithEnum> SupportedEnumDict
            => _registry.SupportedEnumDict;

        public static IReadOnlyDictionary<Type, Type> CEValueTypeDict
            => _registry.CEValueTypeDict;

        public static void RegistBlacksmithEnum(Type type, IBlacksmithEnum instance)
            => _registry.RegistEnum(type, instance);

        public static void RegistBlacksmithEnumModifier(IBlacksmithEnum targetEnum, string name, int priority)
            => _registry.RegistEnumModifier(targetEnum, name, priority);

        private class EnumRegistryInstance
        {
            private readonly Dictionary<Type, IBlacksmithEnum> _supportedEnumDict = new();
            public IReadOnlyDictionary<Type, IBlacksmithEnum> SupportedEnumDict
                => _supportedEnumDict;
            private Dictionary<Type, Type>? _CEValueTypeDict = null;
            private readonly object _ceValueTypeDictLock = new();
            public IReadOnlyDictionary<Type, Type> CEValueTypeDict
            {
                get
                {
                    if (_CEValueTypeDict == null)
                    {
                        lock (_ceValueTypeDictLock)
                        {
                            if (_CEValueTypeDict == null)
                            {
                                _CEValueTypeDict = SupportedEnumDict.ToDictionary(s => s.Key, s => s.Value.GetCEValueType());
                            }
                        }
                    }
                    return _CEValueTypeDict;
                }
            }
            private readonly List<string> _names = new();

            public void RegistEnum(Type type, IBlacksmithEnum instance)
            {
                if (!SupportedEnumDict.TryGetValue(type, out var value) && !_names.Contains(type.Name))
                {
                    _supportedEnumDict[type] = instance;
                    _names.Add(type.Name);
                }
                else
                {
                    throw new ArgumentException($"BlacksmithEnum {type} already exists! Expansion addition failed!");
                }
            }
            public void RegistEnumModifier(IBlacksmithEnum targetEnum, string name, int priority)
            {
                targetEnum.Create(name, priority);
            }
        }
    }
}
