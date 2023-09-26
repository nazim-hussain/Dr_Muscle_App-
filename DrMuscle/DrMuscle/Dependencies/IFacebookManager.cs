using DrMuscle.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscle.Dependencies
{
    public interface IFacebookManager
    {
        Task<bool> SimpleLogin();
        Task<FacebookUser> Login();
        Task LogOut();
        Task<bool> ValidateToken();
        Task<bool> PostText(string message);
        bool ShareText(string text, string link);

    }
}
