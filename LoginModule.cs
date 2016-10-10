using Nancy;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CryptoHelper;

namespace LoginNancy
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get("/", args => {
                if(Session["Errors"] != null)
                {
                    return View["Login/Login.sshtml", Session["Errors"]];
                }
                else
                {
                    return View["Login/Login.sshtml", new List<string>()];
                }

            });

            Post("/registration", args =>  {
                List <string> Errors = new List<string>(); //Create a List to append errors to it
                string first_name = Request.Form.first_name;
                string last_name = Request.Form.last_name;
                string email = Request.Form.email;
                string password = Request.Form.password;
                string confirm_password = Request.Form.confirm_password;
                // Validations start here

                // Fisrt Name
                if(first_name == null)
                {
                    Errors.Add("First Name fileld can not be balnk");
                }
                else if(first_name.Length < 2)
                {
                    Errors.Add("First Name field must be atleast two Characters");
                }
                else if(!Regex.IsMatch(first_name, @"^[a-zA-Z]+$"))
                {
                    Errors.Add("First Nmae should contain only letters");
                }
                // Last Name
                if(last_name == null)
                {
                    Errors.Add("Last Name can not be balnk");
                }
                else if(last_name.Length < 2)
                {
                    Errors.Add("Last Name must be atleast two Characters");
                }
                else if(!Regex.IsMatch(last_name, @"^[a-zA-Z]+$"))
                {
                    Errors.Add("Last Name should contain only letters");
                }
                // email
                if(email == null)
                {
                    Errors.Add("Email Field can not be balnk");
                }
                else if(!Regex.IsMatch(email, @"^[a-zA-Z0-9\.\+_-]+@[a-zA-Z0-9\._-]+\.[a-zA-Z]*$"))
                {
                    Errors.Add("Email should be valid Email Address");
                }
                //password and Confirm - password
                if(password == null)
                {
                    Errors.Add("Password can not be balnk");
                }
                else if(password.Length < 4)
                {
                    Errors.Add("Password must be atleast 4 Characters");
                }
                else if(password != confirm_password)
                {
                    Errors.Add("Confirm Password should match password field");
                }
                // Errors storing to session
                if(Errors.Count > 0)
                {
                    Session["Errors"] = Errors;
                    return Response.AsRedirect("/");
                }
                // If no Errors then append to DB........
                else{
                    string password_Hash = Crypto.HashPassword(password);
                    string Insert_user = $"INSERT INTO users (first_name, last_name, email, password, created_at) VALUES ('{first_name}', '{last_name}','{email}', '{password_Hash}', NOW())";
                    DbConnector.ExecuteQuery(Insert_user);
                    Console.WriteLine("User is added to DB");
                    //Selecting user that was created currently........
                    string query = "SELECT * FROM users ORDER BY id DESC LIMIT 1";
                    List<Dictionary<string, object>> current_user = DbConnector.ExecuteQuery(query);
                    Dictionary<string, object> new_user = current_user[0];
                    Session["currentuser"] = (int)new_user["id"];
                    return Response.AsRedirect("/dashboard");
                }
            });  

            Get("/dashboard", args => {
                //To display on dashboard page... by passing session of current user
                string Query = $"SELECT * FROM users WHERE id = {Session["currentuser"]}";
                List<Dictionary<string, object>> user_fromdb = DbConnector.ExecuteQuery(Query);
                ViewBag.User = user_fromdb[0];
                return View["Success"];
            });

            Post("/login", args => {
                List<string> Errors = new List<string>();
                string email = Request.Form.email;
                string password = Request.Form.password;
                string query = $"SELECT * FROM users WHERE email = '{email}' LIMIT 1";
                List<Dictionary<string, object>> fromdb = DbConnector.ExecuteQuery(query);
                if(fromdb.Count == 0)
                {
                    Errors.Add("Email you entered is not registered");
                    Session["Errors"] = Errors;
                    return Response.AsRedirect("/");
                }
                else
                {
                    Dictionary<string, object> dbUser = fromdb[0];
                    Console.WriteLine(dbUser["password"]); 
                    Console.WriteLine(password); 
                    bool correct = Crypto.VerifyHashedPassword((string) dbUser["password"], password);
                    Console.WriteLine("Hitting2" + correct);
                    if(correct)
                    {
                        Session["currentuser"] = (int)dbUser["id"];
                        return Response.AsRedirect("/dashboard");
                    }
                    else
                    {
                        Errors.Add(" password is incorrect");
                        Session["Errors"] = Errors;
                        return Response.AsRedirect("/");
                    }
                }
            });
        }
    }
}