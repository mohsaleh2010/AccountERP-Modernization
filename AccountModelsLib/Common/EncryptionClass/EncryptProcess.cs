//Stephen Swift
//X00004878
//File: EncryptionProcess.cs
using System;
  


namespace AccountModelsLib  //namespace
{
    /// <summary>
    /// Summary description for EncryptProcess.
    /// </summary>
    internal class EncryptProcess:encrypt	//called from Main.cs
	{	
		//The keyword base is used 
		//to access members of the base class from 
		//inside a derived class. 
		int keey=12021542;
		public EncryptProcess():base("Encrypt Process") //base access

		{}

		~EncryptProcess()			//destructor
		{}

/// <summary>
/// Encrypt Function - Take in text from the inputOutput text box e.b inputOutputDisplay
/// - Take in the cipher key e.g. cipherKeyIn
/// - The cipher key is the value which is used to move the position of the characters in 
///	- the unicode set so (Hello) becomes (mjqqt) if the cipher key is 5 and 
///	- (hello) becomes (ifmmp) if the cipher key is 1
/// </summary>
/// <param name="input"></param>
/// <param name="len"></param>
/// <param name="key"></param>
/// <returns></returns>

		public string encrypt(string input , int key) //encrypt - input, len and key in
		{
			int len=input.Length;
			string encryptedText="";						//string encryptedText blank
			
			for(int i =0; i<len; i++)						//for loop for length of string		
			{
				encryptedText += (char)(int)(input[i]+key); //(char)(int) casting wraps around	 
			}												//final unicode so if cipher key is 
			return encryptedText;							//65536 and the plaintext entered is 'a'
		}													//the ciphertext wraps around to 'a' again

		public string encrypt(string input) //encrypt - input, len and key in
		{
			int len=input.Length;
			 
			string encryptedText="";						//string encryptedText blank
			
			for(int i =0; i<len; i++)						//for loop for length of string		
			{
				encryptedText += (char)(int)(input[i]+keey); //(char)(int) casting wraps around	 
			}												//final unicode so if cipher key is 
			return encryptedText;							//65536 and the plaintext entered is 'a'
		}			
/// <summary>
/// Decrypt Function - Same parameters as encrypt function 
///- The for loop decrements the key so if (mjqqt) is entered and the cipherkey is 5
///- on decrypt (hello) is displayed
/// </summary>
/// <param name="input"></param>
/// <param name="len"></param>
/// <param name="key"></param>
/// <returns></returns>

		public string decrypt(string input,int key)
		{
			int len=input.Length;
			string decryptedText="";							//string decryptedText

			for(int i =0; i<len; i++)							//step through string for length len
			{
				decryptedText += (char)(int)(input[i]-key);		//char int casting array index
			}													//decrement the key
			return decryptedText;
		}


		public string decrypt(string input)
		{
			int len=input.Length;
			string decryptedText="";							//string decryptedText

			for(int i =0; i<len; i++)							//step through string for length len
			{
				decryptedText += (char)(int)(input[i]-keey);		//char int casting array index
			}													//decrement the key
			return decryptedText;
		}


		public string encode(string str)
		{
			string htext = ""; // blank text

			for ( int i = 0; i < str.Length; i++)
			{
				htext = htext + (char) (str[i] + 10 - 1 * 2);
			}
			return htext;
		}

		public string decode(string str)
		{
			string dtext = ""; 

			for ( int i=0; i < str.Length; i++)
			{
				dtext = dtext + (char) (str[i] - 10 + 1*2);
			}
			return dtext;
		}
	}
}
