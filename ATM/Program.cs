using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;

namespace ATM
{
    public class CardHolder
    {
        string cardNum;
        int pin;
        string firstName;
        string lastName;
        double balance;

        public CardHolder(string cardNum, int pin, string firstName, string lastName, double balance)
        {
            this.cardNum = cardNum;
            this.pin = pin;
            this.firstName = firstName;
            this.lastName = lastName;
            this.balance = balance;
        }

        public string GetCardNum()
        {
            return cardNum;
        }

        public int GetPin()
        {
            return pin;
        }

        public string GetFirstName()
        {
            return firstName;
        }

        public string GetLastName()
        {
            return lastName;
        }

        public double GetBalance()
        {
            return balance;
        }

        public void SetCardNum(string newCardNum)
        {
            cardNum = newCardNum;
        }

        public void SetPin(int newPin)
        {
            pin = newPin;
        }

        public void SetFirstName(string newFirstName)
        {
            firstName = newFirstName;
        }

        public void SetLastName(string newLastName)
        {
            lastName = newLastName;
        }

        public void SetBalance(double newBalance)
        {
            balance = newBalance;
        }

        public void Deposit()
        {
            Console.WriteLine("Nhap so tien ban muon gui:");
            double deposit = double.Parse(Console.ReadLine());
            balance += deposit;
            Console.WriteLine("Cam on ban da gui tien. So du moi cua ban la: " + GetBalance());
        }

        public void Withdraw()
        {
            Console.WriteLine("Nhap so tien ban muon rut:");
            double withdrawal = double.Parse(Console.ReadLine());

            // Kiem tra so du
            if (GetBalance() < withdrawal)
            {
                Console.WriteLine("Tai khoan khong du so du! :(");
            }
            else
            {
                SetBalance(GetBalance() - withdrawal);
                Console.WriteLine("Rut tien thanh cong! :)");
            }
        }

        public void Balance()
        {
            Console.WriteLine("So du kha dung: " + GetBalance());
        }

        public static CardHolder CreateAccount()
        {
            Console.WriteLine("Nhap thong tin de tao tai khoan moi:");
            Console.WriteLine("Nhap so the:");
            string cardNum = Console.ReadLine();
            Console.WriteLine("Nhap ma pin:");
            int pin = int.Parse(Console.ReadLine());
            Console.WriteLine("Nhap ho:");
            string firstName = Console.ReadLine();
            Console.WriteLine("Nhap ten:");
            string lastName = Console.ReadLine();
            double balance = 0.0;

            return new CardHolder(cardNum, pin, firstName, lastName, balance);
        }

        public static CardHolder AccessCard(List<CardHolder> cardHolders)
        {
            Console.WriteLine("Vui long nhap ma the ghi no cua ban: ");
            string debitCardNum;

            while (true)
            {
                debitCardNum = Console.ReadLine();

                // Check against our db
                var currentUser = cardHolders.FirstOrDefault(a => a.GetCardNum() == debitCardNum);
                if (currentUser != null)
                {
                    Console.WriteLine("Vui long nhap ma pin cua ban: ");
                    int userPin;
                    while (true)
                    {
                        try
                        {
                            userPin = int.Parse(Console.ReadLine());

                            if (currentUser.GetPin() == userPin) { return currentUser; }
                            else { Console.WriteLine("Ma pin khong chinh xac. Vui long thu lai"); }
                        }
                        catch { Console.WriteLine("Ma pin khong chinh xac. Vui long thu lai"); }
                    }
                }
                else
                {
                    Console.WriteLine("The khong ton tai! Vui long thu lai");
                }
            }
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            void PrintOptions()
            {
                Console.WriteLine("Vui long chon mot trong cac tuy chon sau...");
                Console.WriteLine("1. Gui tien");
                Console.WriteLine("2. Rut tien");
                Console.WriteLine("3. Xem so du");
                Console.WriteLine("4. Thoat");
            }

            List<CardHolder> cardHolders = new List<CardHolder>();
            
            string connectionString = "Data Source=D://ATM//ATM//ATMDatabase.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Tạo bảng nếu nó chưa tồn tại
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS CardHolders (
                        CardNum TEXT PRIMARY KEY,
                        Pin INTEGER,
                        FirstName TEXT,
                        LastName TEXT,
                        Balance REAL
                    )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Đọc dữ liệu từ cơ sở dữ liệu và tạo danh sách cardHolders
                //var cardHolders = new List<CardHolder>();
                string selectQuery = "SELECT * FROM CardHolders";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cardHolders.Add(new CardHolder(
                                reader["CardNum"].ToString(),
                                Convert.ToInt32(reader["Pin"]),
                                reader["FirstName"].ToString(),
                                reader["LastName"].ToString(),
                                Convert.ToDouble(reader["Balance"])
                            ));
                        }
                    }
                }
            }

            // Promt user
            Console.WriteLine("Chao mung den voi MyATM!");
            Console.WriteLine("1. Truy cap tai khoan;");
            Console.WriteLine("2. Tao tai khoan moi.");
            int opt = int.Parse(Console.ReadLine());
            CardHolder currentUser;

            if (opt == 1)
            {
                currentUser = CardHolder.AccessCard(cardHolders);
                Console.WriteLine("Xin chao " + currentUser.GetLastName() + ":)");
                int option = 0;
                do
                {
                    PrintOptions();
                    try
                    {
                        option = int.Parse(Console.ReadLine());
                    }
                    catch { }
                    if (option == 1) { currentUser.Deposit(); }
                    else if (option == 2) { currentUser.Withdraw(); }
                    else if (option == 3) { currentUser.Balance(); }
                    else if (option == 4) { break; }
                    else { option = 0; }
                }
                while (option != 4);
                Console.WriteLine("Cam on! Chuc mot ngay tot lanh :)");
            }
            else if (opt == 2)
            {
                currentUser = CardHolder.CreateAccount();
                Console.WriteLine("Tai khoan da duoc tao!");

                // Thêm tài khoản mới vào cơ sở dữ liệu
                string insertQuery = "INSERT INTO CardHolders (CardNum, Pin, FirstName, LastName, Balance) VALUES (@CardNum, @Pin, @FirstName, @LastName, @Balance)";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CardNum", currentUser.GetCardNum());
                        command.Parameters.AddWithValue("@Pin", currentUser.GetPin());
                        command.Parameters.AddWithValue("@FirstName", currentUser.GetFirstName());
                        command.Parameters.AddWithValue("@LastName", currentUser.GetLastName());
                        command.Parameters.AddWithValue("@Balance", currentUser.GetBalance());
                        command.ExecuteNonQuery();
                    }
                }
            }
         
        }
    }
}