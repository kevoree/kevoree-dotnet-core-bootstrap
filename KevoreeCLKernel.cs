using System;
using System.Linq;
using org.kevoree;
using org.kevoree.impl;
using Org.Kevoree.Core.Api;
using Boolean = java.lang.Boolean;

namespace Org.Kevoree.Core.Bootstrap
{
    public class KevoreeCLKernel : MarshalByRefObject, BootstrapService
    {
        private readonly string nodeName;
        private readonly string nugetLocalRepositoryPath;
        private readonly string nugetRepositoryUrl;
        private Bootstrap bootstrap;
        private KevoreeCoreBean core;

        public KevoreeCLKernel(Bootstrap bootstrap, string nodeName, string nugetLocalRepositoryPath,
            string nugetRepositoryUrl)
        {
            this.bootstrap = bootstrap;
            this.nodeName = nodeName;
            this.nugetLocalRepositoryPath = nugetLocalRepositoryPath;
            this.nugetRepositoryUrl = nugetRepositoryUrl;
        }

        public void injectDictionary(Instance instance, IRunner target, bool defaultOnly)
        {
            if (instance.getTypeDefinition() == null || instance.getTypeDefinition().getDictionaryType() == null)
            {
                return;
            }
            var attrs = instance.getTypeDefinition().getDictionaryType().getAttributes().iterator();
            while (attrs.hasNext())
            {
                var att = (DictionaryAttribute) attrs.next();
                string defValue = null;
                string value = null;
                if (Boolean.TRUE.equals(att.getFragmentDependant()))
                {
                    var fdico = instance.findFragmentDictionaryByID(nodeName);
                    if (fdico != null)
                    {
                        var tempValue = fdico.findValuesByID(att.getName());
                        if (tempValue != null)
                        {
                            value = tempValue.getValue();
                        }
                    }
                }
                if (value == null)
                {
                    if (instance.getDictionary() != null)
                    {
                        var tempValue = instance.getDictionary().findValuesByID(att.getName());
                        if (tempValue != null)
                        {
                            value = tempValue.getValue();
                        }
                    }
                }
                if (att.getDefaultValue() != null && !att.getDefaultValue().Equals(""))
                {
                    defValue = att.getDefaultValue();
                }
                if (defaultOnly)
                {
                    if (defValue != null && value == null)
                    {
                        internalInjectField(att.getName(), defValue, target);
                    }
                }
                else
                {
                    if (value == null && defValue != null)
                    {
                        value = defValue;
                    }
                    if (value != null)
                    {
                        internalInjectField(att.getName(), value, target);
                    }
                }
            }
        }

        /**
         * DEVNOTE : on pose comme prédicat que cette méthode est dédiée à la création d'instance de NODE et rien d'autre
         */

        public INodeRunner createInstance(ContainerNode nodeInstance)
        {
            var typedef = nodeInstance.getTypeDefinition();
            // FIXME : look badly complex for just a DU look (we are looking for the DU of dotnet).
            var deployUnitDotNet =
                ((DeployUnitImpl)
                    typedef.getDeployUnits()
                        .toArray()
                        .Where(x => ((DeployUnitImpl) x).findFiltersByID("platform").getValue() == "dotnet")
                        .First());
            var name = deployUnitDotNet.getName();
            var version = deployUnitDotNet.getVersion();
            var instance = new NugetLoader.NugetLoader(nugetLocalRepositoryPath).LoadRunnerFromPackage<NodeRunner>(
                name, version, nugetRepositoryUrl);
            // TODO : ici injecter les @KevoreeInject dans l'instance
            //var coreProxy = new ContextAwareModelServiceCoreProxy();
            instance.proceedInject(nodeInstance.path(), nodeName, nodeInstance.getName(), core);


            return instance;
        }


        public void setCore(KevoreeCoreBean core)
        {
            this.core = core;
        }

        private bool internalInjectField(string fieldName, string value, object target)
        {
            if (value != null /* && !value.equals("")*/)
            {
                try
                {
                    //bool isSet = false;
                    var setterName = "set";
                    setterName = setterName + fieldName.Substring(0, 1).ToUpper();
                    if (fieldName.Count() > 1)
                    {
                        setterName = setterName + fieldName.Substring(1);
                    }
                    //MethodInfo setter = lookupSetter(setterName, target.getClass());
                    /*if (setter != null && setter.getParameterTypes().length == 1) {
                        if (!setter.isAccessible()) {
                            setter.setAccessible(true);
                        }
                        Class pClazz = setter.getParameterTypes()[0];
                        if (pClazz.equals(boolean.class)) {
                            setter.invoke(target, Boolean.parseBoolean(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Boolean.class)) {
                            setter.invoke(target, new Boolean(Boolean.parseBoolean(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(int.class)) {
                            setter.invoke(target, Integer.parseInt(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Integer.class)) {
                            setter.invoke(target, new Integer(Integer.parseInt(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(long.class)) {
                            setter.invoke(target, Long.parseLong(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Long.class)) {
                            setter.invoke(target, new Long(Long.parseLong(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(double.class)) {
                            setter.invoke(target, Double.parseDouble(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Double.class)) {
                            setter.invoke(target, new Double(Double.parseDouble(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(String.class)) {
                            setter.invoke(target, value);
                            isSet = true;
                        }
                        if (pClazz.equals(short.class)) {
                            setter.invoke(target, Short.parseShort(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Short.class)) {
                            setter.invoke(target, new Short(Short.parseShort(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(float.class)) {
                            setter.invoke(target, Float.parseFloat(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Float.class)) {
                            setter.invoke(target, new Float(Float.parseFloat(value)));
                            isSet = true;
                        }
                        if (pClazz.equals(byte.class)) {
                            setter.invoke(target, Byte.parseByte(value));
                            isSet = true;
                        }
                        if (pClazz.equals(Byte.class)) {
                            setter.invoke(target, new Byte(Byte.parseByte(value)));
                            isSet = true;
                        }
                        if (value.length() == 1) {
                            if (pClazz.equals(char.class)) {
                                setter.invoke(target, value.charAt(0));
                                isSet = true;
                            }
                        }
                    }*/

                    /*if (!isSet) {
                        Field f = lookup(fieldName, target.getClass());
                        if (!f.isAccessible()) {
                            f.setAccessible(true);
                        }
                        if (f.getType().equals(boolean.class)) {
                            f.setBoolean(target, Boolean.parseBoolean(value));
                        }
                        if (f.getType().equals(Boolean.class)) {
                            f.set(target, new Boolean(Boolean.parseBoolean(value)));
                        }
                        if (f.getType().equals(int.class)) {
                            f.setInt(target, Integer.parseInt(value));
                        }
                        if (f.getType().equals(Integer.class)) {
                            f.set(target, new Integer(Integer.parseInt(value)));
                        }
                        if (f.getType().equals(long.class)) {
                            f.setLong(target, Long.parseLong(value));
                        }
                        if (f.getType().equals(Long.class)) {
                            f.set(target, new Long(Long.parseLong(value)));
                        }
                        if (f.getType().equals(double.class)) {
                            f.setDouble(target, Double.parseDouble(value));
                        }
                        if (f.getType().equals(Double.class)) {
                            f.set(target, new Double(Double.parseDouble(value)));
                        }
                        if (f.getType().equals(String.class)) {
                            f.set(target, value);
                        }
                        if (f.getType().equals(short.class)) {
                            f.set(target, Short.parseShort(value));
                        }
                        if (f.getType().equals(Short.class)) {
                            f.set(target, new Short(Short.parseShort(value)));
                        }
                        if (f.getType().equals(float.class)) {
                            f.set(target, Float.parseFloat(value));
                        }
                        if (f.getType().equals(Float.class)) {
                            f.set(target, new Float(Float.parseFloat(value)));
                        }
                        if (f.getType().equals(byte.class)) {
                            f.set(target, Byte.parseByte(value));
                        }
                        if (f.getType().equals(Byte.class)) {
                            f.set(target, new Byte(Byte.parseByte(value)));
                        }
                        if (value.length() == 1) {
                            if (f.getType().equals(char.class)) {
                                f.set(target, value.charAt(0));
                            }
                        }
                    }*/
                    return true;
                }
                catch (Exception)
                {
                    //Log.error("No field corresponding to annotation, consistency error {} on {}", e, fieldName, target.getClass().getName());
                    return false;
                }
            }
            return false;
        }
    }
}