using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscle.Dependencies
{
    public delegate void FBLoginSucceded(string id, string email, string gender, string token);

    public interface ILoginFB
    {
        string Login();
        event FBLoginSucceded OnFBLoginSucceded;
    }
}
