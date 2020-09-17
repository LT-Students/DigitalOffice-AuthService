using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.AuthentificationService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// </summary>
    public interface IForgotPasswordCommand
    {
        /// <summary>
        ///Method for getting user id and jwt by email and password
        /// </summary>
        /// <param name="request">Request model with user email and password.</param>
        /// <returns>Response model with user id and jwt</returns>
        void Execute(string userEmail);
    }
}
