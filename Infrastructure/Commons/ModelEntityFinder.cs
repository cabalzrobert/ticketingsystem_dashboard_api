using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Commons
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true)]
    public class ModelEntityFinder: Attribute
    {
        // settings
        public bool ColumnAttributeOnly = true;
        public static void Init(ModelBuilder builder)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = (from type in assembly.GetTypes()
                        where Attribute.IsDefined(type, typeof(ModelEntityFinder))
                        select type).ToList();
            if(types.Count != 0)
            {
                Type typeOfFinder = typeof(ModelEntityFinder);
                MethodInfo mToEntity = typeOfFinder.GetMethod("ToEntity");  
                types.ForEach(t=>
                {
                    bool ColumnAttributeOnly = true;
                    t.GetCustomAttributes(true).ToList().ForEach(a=>
                    {
                        if (a is ModelEntityFinder)
                            ColumnAttributeOnly = (a as ModelEntityFinder).ColumnAttributeOnly;
                    });
                    EntityTypeBuilder entity = builder.Entity(t);    
                    mToEntity.MakeGenericMethod(t).Invoke(null, new object[]{ entity, ColumnAttributeOnly });
                });
            }
            // method Only
            var methods = (from type in assembly.GetTypes()
                            from method in type.GetMethods()
                            where (!types.Contains(type) && Attribute.IsDefined(method, typeof(ModelEntityFinder)))
                                select method).ToList();
            if(methods.Count != 0)
            {
                methods.ForEach(m=> m.Invoke(null, new object[] { builder }));
            }
        }
        public static void ToEntity<T>(EntityTypeBuilder entity, bool IsColumnAttributeOnly = true)
        {
            Type type = typeof(T);
            string TableName = type.Name;
            type.GetCustomAttributes(true).ToList().ForEach(a=>
            {
                if (a is TableAttribute) 
                    TableName = (a as TableAttribute).Name;
            });
            entity.ToTable(TableName);
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if(IsColumnAttributeOnly){
                properties = (from property in type.GetProperties()
                    where (Attribute.IsDefined(property, typeof(ColumnAttribute))
                        ||Attribute.IsDefined(property, typeof(NotMappedAttribute))
                        ||Attribute.IsDefined(property, typeof(KeyAttribute))
                    ) select property).ToList();
            }
            if(properties.Count == 0 || !IsColumnAttributeOnly){
                properties = (from property in type.GetProperties()
                    select property).ToList();
            }
            properties.ToList().ForEach(p=>
            {
                string columnName = null, columnType = null;
                bool isNotMapped = false, isKey = false;
                p.GetCustomAttributes(true).ToList().ForEach(a=>
                {
                    if (a is ColumnAttribute) {
                        var colAttr = (a as ColumnAttribute);
                        if(!String.IsNullOrEmpty(colAttr.Name)) columnName =  colAttr.Name;
                        if(!String.IsNullOrEmpty(colAttr.TypeName)) columnType = colAttr.TypeName;
                    }
                    if (a is NotMappedAttribute) 
                        isNotMapped = true;
                    if (a is KeyAttribute) 
                        isKey = true;
                });
                if(isNotMapped)
                {
                   //entity.HasOne(p.Name);
                   entity.Ignore(p.Name);
                }
                else 
                {
                    var prop = entity.Property(p.Name);
                    
                    if(!String.IsNullOrEmpty(columnName))
                        prop.HasColumnName(columnName);
                    else columnName = p.Name;
                    if(!String.IsNullOrEmpty(columnType))
                        prop.HasColumnType(columnType);
                    if(isKey) prop.ValueGeneratedOnAdd()
                                .HasAnnotation("Key", 0);
                    /* if(isKey && false) entity.HasKey(columnName); */
                }
            });
        }
    }
}