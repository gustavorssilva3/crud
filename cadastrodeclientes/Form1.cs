using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace cadastrodeclientes
{
    public partial class frmCadastrodeClientes : Form
    {
        //Conexão com o banco de dados MySQL
        MySqlConnection Conexao;
        string data_source = "datasource=localhost; username=root; password=; database=db_cadastro";
        public frmCadastrodeClientes()
        {
            InitializeComponent();
        }

        private bool isValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        private bool isValidCPFLength(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            {
                return false;
            }
            return true;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validação dos campos obrigatórios
                if (string.IsNullOrEmpty(txtNomeCompleto.Text.Trim()) ||
                    string.IsNullOrEmpty(txtEmail.Text.Trim()) ||
                    string.IsNullOrEmpty(txtCPF.Text.Trim()))
                {
                    MessageBox.Show("Todos os campos devem ser preenchidos.",
                                    "Validação",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validação Email
                string email = txtEmail.Text.Trim();
                if (!isValidEmail(email))
                {
                    MessageBox.Show("E-mail inválido. Certifique-se de que o e-mail está no formato correto.",
                                    "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validação CPF
                string cpf = txtCPF.Text.Trim();
                if (!isValidCPFLength(cpf))
                {
                    MessageBox.Show("CPF inválido. Certifique-se que o CPF tem 11 dígitos numéricos.",
                                    "Validação",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //Cria a conexão com o banco de dados
                Conexao = new MySqlConnection(data_source);
                Conexao.Open();
                // Teste de abertura
                // MessageBox.Show("Conexão aberta com sucesso");

                //Comando SQL para inserir um novo cliente no banco de dados
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = Conexao
                };

                cmd.Prepare();

                cmd.CommandText = "INSERT INTO dadosdecliente(nomecompleto, nomesocial, email, cpf) " +
                    "VALUES (@nomecompleto, @nomesocial, @email, @cpf)";

                //Adiciona parâmetros com os dados do formulário
                cmd.Parameters.AddWithValue("@nomecompleto", txtNomeCompleto.Text.Trim());
                cmd.Parameters.AddWithValue("@nomesocial", txtNomeSocial.Text.Trim());
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@cpf", cpf);

                // Executa o comando de inserção no bando de dados
                cmd.ExecuteNonQuery();

                //Menssagem de sucesso
                MessageBox.Show("Contato inserido com sucesso: ", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            catch(MySqlException ex)
            {
                //Trata erros relacionados ao SQL
                MessageBox.Show("Erro " + ex.Number + " ocorreu " + ex.Message, 
                    "Erro",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }

            catch (Exception ex)
            {
                // Trata outros tipos de erro
                MessageBox.Show("Ocorreu: " + ex.Message, 
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                //Garante que a conexão com banco de dados será fechada mesmo se ocorrer erro
                if(Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                    // Teste de Fechamento
                    // MessageBox.Show("Conexão fechada com sucesso");
                }
            }
        }
    }
}
