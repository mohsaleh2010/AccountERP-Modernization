//Stephen Swift
//X00004878
//File: encrypt.cs

using System;

namespace AccountModelsLib //The Project
{
	/// <summary>
	///This is the base class
	///A class marked as abstract 
	///cannot be instantiated, and has to be 
	///inherited by another class. 
	/// </summary>
	///				
	 internal abstract class encrypt					//abstract class
	{
		protected string  encryptionType;			//the type of encryption 

		public encrypt(string encryptionType)		//pass in string encryptionType
		{
			this.encryptionType = encryptionType;	//refer to the current object
		}

		public encrypt():this("")					//this object
		{}

		~encrypt()									//destructor
		{}
	}
}
