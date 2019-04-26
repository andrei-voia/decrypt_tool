using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecryptTool
{
    public partial class Form1 : Form
    {
        //defined values
        private const int MODULE_VALUE = 125;
        private const int LABEL_RESET_LOCATION = 232;
        //input initialized values
        private string decrypted = "";
        private string encrypted = "";
        private string key = "";

        public Form1()
        {
            InitializeComponent();
            //set the decrypted box to non writable
            richTextBox2.ReadOnly = true;
            richTextBox2.BackColor = SystemColors.Window;
            label4.Text = "(Length: " + richTextBox1.Text.Length + ")";
            label4.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label4.Text, label4.Font).Width;
            label6.Text = "(Length: " + richTextBox2.Text.Length + ")";
            label6.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label6.Text, label6.Font).Width;
            label5.Text = "";  
        }  

        //DECRYPTION method
        private string decrypt(string encrypted, string key)
        {
            string decrypted = "";
            decrypted = encrypted;
            int randomNumber = 0;
            int[] order = new int[5];
            int choice = 0;
            int difficulty = 0;

            //extrats the difficulty at which the encryption will be decripted, between 1 and 3
            decrypted = extractDifficulty(decrypted, ref difficulty);

            decrypted = getAdditionalKey(decrypted, difficulty, ref key);
            //if there is no key input
            if (decrypted == null) return null;

            if (difficulty > 1)
            {
                for (int i = 0; i < key.Length; i++)
                    if (key[i] % 2 == 1) choice += key[i];

                if (choice % 2 == 0)
                {
                    decrypted = interchangeBits(decrypted, key);
                    if (difficulty > 2) decrypted = scatterBits(decrypted, key);
                }
                else
                {
                    decrypted = scatterBits(decrypted, key);
                    if (difficulty > 2) decrypted = interchangeBits(decrypted, key);
                }

                decrypted = converToDecimal(decrypted);
            }

            decrypted = extractRandom(decrypted, key, ref randomNumber);

            if (difficulty > 2) decrypted = decryptMatrix(decrypted, key, randomNumber);

            if (difficulty > 2) decrypted = extractRandomizedASCII(decrypted);
            //change to random
            if (difficulty > 1) decrypted = reverseXOR(decrypted, key, randomNumber);

            setOrder(ref order, key);

            for (int i = 4; i >= 1; i--)
            {
                if (order[i] == 1) decrypted = shiftRight(decrypted, (key.Length % 6));
                else if (order[i] == 2) decrypted = reverse(decrypted);
                else if (order[i] == 3) decrypted = incrementASCII(decrypted, -1 * (1 + key.Length % 5));   //reverse of the incrementation (decrement)
                else if (order[i] == 4) if (difficulty > 2)
                    {
                        int distance = 2 + key.Length % 5 - key.Length / 10;
                        int offset = 1 + key.Length % 3;
                        if (distance < 0) distance = -1 * distance;
                        if (offset < 0) offset = -1 * offset;
                        if (distance < offset)
                        {
                            int aux = distance;
                            distance = offset;
                            offset = aux;
                        }

                        decrypted = scatter(decrypted, distance, offset);
                    }
            }

            return decrypted;
        }

        //DECRYPTING
        #region decrypting process
        private string getAdditionalKey(string encrypted, int difficulty, ref string key)
        {
            char[] localCharacter = encrypted.ToCharArray();

            if (difficulty == 1)
            {
                bool shouldAdd;
                if ((int)localCharacter[localCharacter.Length - 1] % 2 == 0) shouldAdd = false;
                else shouldAdd = true;

                //if we don't check no-key checkbox
                if (shouldAdd == false)
                {
                    Array.Resize(ref localCharacter, localCharacter.Length - 1);
                    if (checkKey() == false) return null;
                }

                //if we check no-key checkbox
                else
                {
                    //reset the key because it is a No-key decryption
                    textBox1.Text = "";
                    MessageBox.Show("No-Key encryption type found.\nPorceeding to decrypt.");

                    int length = (int)localCharacter[localCharacter.Length - 2] - 33;
                    char[] newKey = new char[length];

                    for (int i = 0; i < length; i++)
                    {
                        newKey[i] = (char)((int)localCharacter[localCharacter.Length - i - 3] - 5);
                    }

                    key = new string(newKey);
                    Array.Resize(ref localCharacter, localCharacter.Length - length - 2);
                }
            }

            else
            {
                if (localCharacter[localCharacter.Length - 1] == '0')
                {
                    Array.Resize(ref localCharacter, localCharacter.Length - 1);
                    if (checkKey() == false) return null;
                }

                else
                {
                    //reset the key because it is a No-key decryption
                    textBox1.Text = "";
                    MessageBox.Show("No-Key encryption type found.\nPorceeding to decrypt.");

                    //delete the last character which tells you that this is a NO-KEY type
                    Array.Resize(ref localCharacter, localCharacter.Length - 1);

                    //set the length
                    char[] lungime = new char[8];
                    for (int i = 0; i < 8; i++)
                    {
                        lungime[i] = localCharacter[localCharacter.Length - 8 + i];
                    }

                    string aux = converToDecimal(new string(lungime));
                    int length = (int)aux[0] - 48;

                    //set the key
                    char[] lungime2 = new char[8 * length];
                    for (int i = 0; i < 8 * length; i++)
                    {
                        lungime2[i] = localCharacter[localCharacter.Length - 8 - 8 * length + i];
                    }

                    aux = converToDecimal(new string(lungime2));

                    key = aux;
                    Array.Resize(ref localCharacter, localCharacter.Length - 8 - 8 * length);
                }
            }

            return new string(localCharacter);
        }

        private string extractDifficulty(string encrypted, ref int difficulty)
        {
            char[] localCharacter = encrypted.ToCharArray();

            if ((localCharacter[localCharacter.Length - 1] == (char)(48) ||
                localCharacter[localCharacter.Length - 1] == (char)(49)) &&
                (localCharacter[localCharacter.Length - 2] == (char)(48) ||
                localCharacter[localCharacter.Length - 2] == (char)(49)))
            {
                int first = (int)localCharacter[localCharacter.Length - 2] - 48;
                int second = (int)localCharacter[localCharacter.Length - 1] - 48;

                if (first == 0 && second == 0) difficulty = 1;
                else if (first == 0 && second == 1) difficulty = 2;
                else if (first == 1 && second == 0) difficulty = 3;
                else
                {
                    MessageBox.Show("Error while trying to extract difficulty.");
                    this.Close();
                }

                Array.Resize(ref localCharacter, localCharacter.Length - 2);
            }

            else
            {
                int number = (int)localCharacter[localCharacter.Length - 1];

                if (50 <= number && number <= 75) difficulty = 1;
                else if (75M <= number && number <= 100) difficulty = 2;
                else if (101 <= number && number <= 126) difficulty = 3;
                else
                {
                    //for safety purposes
                    MessageBox.Show("Error in setting the difficulty.");
                    this.Close();
                }

                Array.Resize(ref localCharacter, localCharacter.Length - 1);
            }

            return new string(localCharacter);
        }

        private string shiftRight(string decrypted, int n)
        {
            char[] localCharacter = decrypted.ToCharArray();

            int i;
            for (i = 0; i < localCharacter.Length; i++)
            {
                if (i - n >= 0)
                    localCharacter[i] = decrypted[i - n];
                else
                    localCharacter[i] = decrypted[localCharacter.Length + i - n];
            }

            return new string(localCharacter);
        }

        private string reverseXOR(string decrypted, string key, int random)
        {
            char[] localCharacter = decrypted.ToCharArray();

            int value = 0;
            int countWords = random + random % 10;

            //calculate stuff
            for (int i = 0; i < key.Length; i++) value += key[i];
            while (value >= random * 3 + value % 10) value /= 2;

            int count = 0;

            for (int i = 0; i < localCharacter.Length; i++)
            {
                int module = MODULE_VALUE;   //125 works
                if (localCharacter[i] == '!')
                {
                    count++;

                    for (int j = i; j < localCharacter.Length - 1; j++)
                    {
                        localCharacter[j] = localCharacter[j + 1];
                    }

                    Array.Resize(ref localCharacter, localCharacter.Length - 1);
                    i--;
                }

                //this  happens when you have actual encrypted code
                else
                {
                    countWords++;
                    //set the module
                    module = module - (5 * count);

                    localCharacter[i] = (char)(localCharacter[i] ^ ((countWords + value) % module));

                    count = 0;
                }
            }

            return new string(localCharacter);
        }

        private string extractRandomizedASCII(string decrypted)
        {
            char[] localCharacter = decrypted.ToCharArray();

            int length = 0;
            int i;
            for (i = 0; i < localCharacter.Length; i++)
            {
                if (i == 0)
                {
                    length = localCharacter[i] - 34;
                    continue;
                }

                if ((int)localCharacter[i] - 1 - i % length >= 33)
                    localCharacter[i] = (char)((int)localCharacter[i] - 1 - i % length);

                else
                {
                    int x;
                    int y;
                    x = localCharacter[i] - 33;
                    y = 126 + x;
                    localCharacter[i] = (char)(y - 1 - i % length);
                }
            }
            deleteIncluded(ref localCharacter, 0);

            return new string(localCharacter);
        }

        private void deleteIncluded(ref char[] localCharacter, int position)
        {
            for (int i = position; i < localCharacter.Length - 1; i++)
            {
                localCharacter[i] = localCharacter[i + 1];
            }
            Array.Resize(ref localCharacter, localCharacter.Length - 1);
        }

        private string extractRandom(string decrypted, string key, ref int randomNumber)
        {
            char[] localCharacter = decrypted.ToCharArray();

            int random;
            int randomFirst, randomSecond;
            int second = localCharacter[0];
            int first = localCharacter[localCharacter.Length - 1];

            deleteIncluded(ref localCharacter, 0);
            deleteIncluded(ref localCharacter, localCharacter.Length - 1);

            int a;
            int mode = 1;
            int value = (int)key[0] / 10;

            int i;
            for (i = 1; i < key.Length; i++)
            {
                if (mode == 1) value *= (int)key[i] / 10;
                else value -= (int)key[i] / 10;

                mode = -1 * mode;
            }

            a = value % 10;
            randomFirst = ((first - 30) / 4 - a);
            randomSecond = (second - 120) / (-4) - a;

            random = randomFirst * 10 + randomSecond;

            randomNumber = random;
            return new string(localCharacter);
        }

        private string converToDecimal(string decrypted)
        {
            char[] localCharacter = decrypted.ToCharArray();
            int countBinary = 0;
            int i, j;
            char[] binaryCharacter = new char[decrypted.Length / 8];

            for (i = 0; i < localCharacter.Length; i = i + 8)
            {
                int[] bin = new int[8];
                int count = 7;

                for (j = i; j <= i + 7; j++)
                {
                    bin[count] = (int)localCharacter[j] - 48;
                    count--;
                }

                //make it decimal
                int dec = 0;
                for (j = 0; j < 8; j++)
                {
                    if (bin[j] == 1)
                        dec += getPow(2, j);
                }

                //save it in decimal string
                binaryCharacter[countBinary] = (char)dec;
                countBinary++;
            }

            return new string(binaryCharacter);
        }

        private string decryptMatrix(string decrypted, string key, int random)
        {
            char[] localCharacter = decrypted.ToCharArray();

            int i, j;
            int offset = 1 + (random % 10) / 3;
            int start = 1 + (random / 10) * 2;
            int len = 0;

            //setting the width
            for (i = 0; i < key.Length; i++) len += (int)key[i];

            if (random < 40) len = len % random;
            else len = len % random / 3;

            int m = 10 + len;
            int n = localCharacter.Length / m;

            if (start >= m) start = (random / 10);
            if (start >= m) start = 1;

            char[,] matrix = new char[m, n];

            for (j = 0; j < n; j++)
            {
                for (i = 0; i < m; i++)
                {
                    matrix[i, j] = '\n';
                }
            }

            int count = 0;
            for (j = 0; j < n; j++)
            {
                for (i = 0; i < m; i++)
                {
                    matrix[i, j] = localCharacter[count];
                    count++;
                }
            }

            char[] save = new char[n];
            //rebuild pointer
            for (j = 0; j < n; j++)
            {
                save[j] = matrix[start, j];
                start = start + (int)(matrix[(start + offset) % m, j] - 'N');
            }

            return new string(save);
        }

        private int getPow(int a, int power)
        {
            int i;
            if (power == 0) return 1;
            int sum = 1;

            for (i = 1; i <= power; i++)
                sum = sum * a;

            return sum;
        }

        private string scatterBits(string encrypted, string key)
        {
            char[] localCharacter = encrypted.ToCharArray();

            int i;
            int coordinate;
            int sum = 0;
            for (i = 0; i < key.Length; i++)
                if ((int)key[i] % 2 == 0)
                    sum += key[i];

            coordinate = sum / key.Length;
            if (coordinate < 1) coordinate = 1;

            while (coordinate < localCharacter.Length)
            {
                for (i = 0; i < key.Length; i++)
                {
                    if (coordinate + i >= localCharacter.Length) break;
                    if (localCharacter[coordinate + i] == '0') localCharacter[coordinate + i] = '1';
                    else localCharacter[coordinate + i] = '0';
                }
                coordinate += coordinate;
            }

            return new string(localCharacter);
        }

        private string interchangeBits(string encrypted, string key)
        {
            char[] localCharacter = encrypted.ToCharArray();

            int i;
            int offset = 1 + localCharacter.Length % key.Length;
            int advance = 1 + key.Length;

            for (i = offset; i < localCharacter.Length; i = i + advance)
            {
                if (i + offset >= localCharacter.Length) break;
                char aux = localCharacter[i];
                localCharacter[i] = localCharacter[i + offset];
                localCharacter[i + offset] = aux;
            }

            return new string(localCharacter);
        }

        private string reverse(string encrypted)
        {
            char[] localCharacter = encrypted.ToCharArray();

            for (int i = 0; i < localCharacter.Length / 2; i++)
            {
                char aux = localCharacter[i];
                localCharacter[i] = localCharacter[localCharacter.Length - 1 - i];
                localCharacter[localCharacter.Length - 1 - i] = aux;
            }

            return new string(localCharacter);
        }

        private string incrementASCII(string encrypted, int n)
        {
            char[] localCharacter = encrypted.ToCharArray();

            for (int i = 0; i < localCharacter.Length; i++)
            {
                localCharacter[i] = (char)((int)localCharacter[i] + n);
            }

            return new string(localCharacter);
        }

        private string scatter(string encrypted, int distance, int offset)
        {
            char[] localCharacter = encrypted.ToCharArray();

            for (int i = distance; i < localCharacter.Length; i = i + distance + offset)
            {
                char aux = localCharacter[i];
                localCharacter[i] = localCharacter[i - distance];
                localCharacter[i - distance] = aux;
            }

            return new string(localCharacter);
        }

        private void setOrder(ref int[] order, string key)   //0, 1, 2 and 3
        {
            order[1] = 0;
            order[2] = 0;
            order[3] = 0;
            order[4] = 0;

            if (key.Length < 3)
            {
                order[1] = 1;   //shiftLeft
                order[2] = 2;   //reverse
                order[3] = 3;   //incrementASCII
                order[4] = 4;   //scatter
                return;
            }

            int space = key.Length / 3;
            int i;
            for (i = 1; i <= 3; i++)
            {
                char a = key[i * space - 1];
                int rest = (int)a % 4 + 1;

                setElement(ref order, rest, i);
            }
            setElement(ref order, 1, 4);
        }

        private void setElement(ref int[] order, int rest, int i)
        {
            if (order[rest] == 0) order[rest] = i;

            else if (order[1] == 0) order[1] = i;
            else if (order[2] == 0) order[2] = i;
            else if (order[3] == 0) order[3] = i;
            else if (order[4] == 0) order[4] = i;
        }
        #endregion

        //here are basic functions used in the buttons
        #region used functions
        //find out if the text and key are wrote down
        private bool checkText()
        {
            if (richTextBox1.Text == "")
            {
                MessageBox.Show("No encrypted input text found.");
                return false;
            }
            return true;
        }

        //find out if the key is there or not
        private bool checkKey()
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("No key found.");
                return false;
            }
            return true;
        }

        //delete the last character
        private string deleteLastCharacter(string textBox)
        {
            return textBox.Substring(0, textBox.Length - 1);
        }
        #endregion

        //programs the buttons to do specific tasks 
        #region buttons and updating functions
        //updating the input text box
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            label4.Text = "(Length: " + richTextBox1.Text.Length + ")";
            label4.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label4.Text, label4.Font).Width;
        }

        //DECRYPT button, it decrypts the input encrypted text
        private void button1_Click(object sender, EventArgs e)
        {
            //refresh the screen stuff
            label5.Text = "Loading...";
            richTextBox2.Text = "";
            this.Refresh();

            if (checkText() == false)
            {
                label5.Text = "";
                return;
            }

            //if the last character is '\n' from copy-paste then it will be deleted
            if (richTextBox1.Text[richTextBox1.Text.Length - 1] == '\n')
            {
                MessageBox.Show("Warning: Last character is an 'enter' character.\nPlease make sure it does not end with 'enter'.");
                richTextBox1.Text = deleteLastCharacter(richTextBox1.Text);
            }

            try
            {
                encrypted = richTextBox1.Text;
                key = textBox1.Text;

                //decrypt the encrypted text
                decrypted = decrypt(encrypted, key);
                //if there is no key input
                if (decrypted == null)
                {
                    label5.Text = "";
                    return;
                }
                //show the decrypted text
                richTextBox2.Text = decrypted;
                label6.Text = "(Length: " + richTextBox2.Text.Length + ")";
                label6.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label6.Text, label6.Font).Width;
            }
            catch
            {
                MessageBox.Show("An error occurred while trying to decrypt text.");
                label5.Text = "";
                return;
            }

            label5.Text = "Done!";
            return;
        }

        //REFRESH button
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            textBox1.Text = "";

            label4.Text = "(Length: " + richTextBox1.Text.Length + ")";
            label4.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label4.Text, label4.Font).Width;
            label6.Text = "(Length: " + richTextBox2.Text.Length + ")";
            label6.Left = LABEL_RESET_LOCATION - TextRenderer.MeasureText(label6.Text, label6.Font).Width;
            label5.Text = "";
        }

        //SAVE TO FILE button
        private void button5_Click(object sender, EventArgs e)
        {
            label5.Text = "Loading...";

            string path = System.Windows.Forms.Application.StartupPath;
            path += "\\Decrypted.txt";

            if (richTextBox2.Text == "")
            {
                MessageBox.Show("No Decrypted text found.");
                label5.Text = "";
                return;
            }

            try
            {
                System.IO.File.WriteAllText(path, richTextBox2.Text);
                MessageBox.Show("Operation ended succesfully.\nDecrypted text copied to \"\\Decrypted.txt\"");
            }
            catch
            {
                MessageBox.Show("Error while trying to copy Decrypted text in folder.");
                label5.Text = "";
                return;
            }

            label5.Text = "Done!";
        }

        //READ FROM FILE option
        private void button4_Click(object sender, EventArgs e)
        {
            label5.Text = "Loading...";

            string path = System.Windows.Forms.Application.StartupPath;
            path += "\\Encrypted.txt";
            string text = "";

            try
            {
                text = System.IO.File.ReadAllText(path);
                if (text == "")
                {
                    MessageBox.Show("Encrypted input file is empty.");
                    label5.Text = "";
                    return;
                }

                richTextBox1.Text = text;

                MessageBox.Show("Operation ended succesfully.\nText copied from \"\\Encrypted.txt\"");
            }
            catch
            {
                MessageBox.Show("Error while trying to copy text from folder.\n\"\\Encrypted.txt\" was not found.");
                label5.Text = "";
                return;
            }

            label5.Text = "Done!";
        }
        #endregion
    }
}
