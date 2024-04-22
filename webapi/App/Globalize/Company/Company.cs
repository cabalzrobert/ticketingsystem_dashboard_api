using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using webapi.Services.Dependency;
using webapi.Commons.AutoRegister;

namespace webapi.App.Globalize.Company
{
    [Service.ITransient(typeof(Company))] 
    public interface ICompany
    {
        Dictionary<string, Dictionary<string, object>> OperatorTypes();
        bool IsOperatorID(string ID);
        string CompanyID();
        string BranchID();
    }

    public class Company: ICompany
    {   
        private readonly IFileData _fd;
        public Company(IFileData fd){
            _fd = fd;
        }
        private Dictionary<string, Dictionary<string, object>> GetUserTypes(){
            return _fd.Data<Dictionary<string, Dictionary<string, object>>>("Company:UserTypes", false);
        }
        
        public Dictionary<string, Dictionary<string, object>> OperatorTypes(){
            return GetUserTypes().Where(kvp=> kvp.Value.Get<bool>("Office", false)).ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
        }
        public bool IsOperatorID(string ID){
            return GetUserTypes().Where(kvp=> kvp.Key.Equals(ID) && kvp.Value.Get<bool>("Office", false)).Count() != 0;
        }
        public string CompanyID(){
            return _fd.String("Company:ID");
        }
        public string BranchID(){
            return _fd.String("Company:Branch:ID");
        }
        /*public List<object> OperatorTypes(){
            return GetUserTypes().Where(kvp=> kvp.Value.Get<bool>("Office", false)).Select(kvp=>kvp.Value).ToList<object>();
        }*/
    }
}