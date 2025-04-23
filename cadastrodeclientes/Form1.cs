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

        private int? codigo_cliente = null;

        public frmCadastrodeClientes()
        {
            InitializeComponent();

            //Configuração inicial do listview para exibição dos dados dos clientes
            lstCliente.View = View.Details;            //Define a visualizaão como "Detalhes
            lstCliente.LabelEdit = true;               // Permite editar os titulos das colunas
            lstCliente.AllowColumnReorder = true;      // Permite reordenar as colunas
            lstCliente.FullRowSelect = true;           // Seleciona linhas inteiras ao clicar
            lstCliente.GridLines = true;               // Exibe linhas de grade no listview

            //Definando as colunas do listview
            lstCliente.Columns.Add("Codígo", 100, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Completo", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Social", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("E-mail", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("CPF", 200, HorizontalAlignment.Left);

            //Carrega os dados dos clientes na interface
            carregar_clientes();
        }

        private void carregar_clientes_com_query(string query)
        {
            try
            {
                //Cria conexão com banco de dados
                Conexao = new MySqlConnection(data_source);
                Conexao.Open();

                //Executa o a consulta SQL fornecida
                MySqlCommand cmd = new MySqlCommand(query, Conexao);

                //Se a consulta contém parametro @q, adiciona o valor da caixa de pesquisa
                if (query.Contains("@q"))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + txtBuscar.Text + "%");
                }

                //Exucuta o comando e obtém os resultado
                MySqlDataReader reader = cmd.ExecuteReader();

                //Limpa os itens existentes no Listview antes de adicionar novos
                lstCliente.Items.Clear();

                // Preenche o Listview com os dados do cliente
                while(reader.Read())
                {
                    //Cria uma linha para cada cliente com os dados retornados da consulta
                    string[] row =
                    {
                        Convert.ToString(reader.GetInt32(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    };

                    //Adiciona a linha ao Listview
                    lstCliente.Items.Add(new ListViewItem(row));
                }
            }

            catch(MySqlException ex)
            {
                //Trata erros realacionados ao MySQL
                MessageBox.Show("Erro " + ex.Message + " ocorreu: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ocorreu " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                }
            }
        }

        //Metodo para carregar todos os clientes no Listview (usando uma consulta sem parametros
        private void carregar_clientes()
        {
            string query = "SELECT * FROM dadosdecliente ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }

        //Regex
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

                if (codigo_cliente == null)
                {
                    // Insert
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
                    MessageBox.Show("Contato inserido com sucesso: ",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    // UPDATE
                    cmd.CommandText = $"UPDATE `dadosdecliente` SET " +
                    $"nomecompleto = @nomecompleto, " +
                    $"nomesocial = @nomesocial, " +
                    $"email = @email, " +
                    $"cpf = @cpf " +
                    $"WHERE codigo = @codigo";

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@nomecompleto", txtNomeCompleto.Text.Trim());
                    cmd.Parameters.AddWithValue("@nomesocial", txtNomeSocial.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@cpf", cpf);
                    cmd.Parameters.AddWithValue("@codigo", codigo_cliente);

                    //Executa o comando de alteração no banco
                    cmd.ExecuteNonQuery();

                    //Menssagem de sucesso para dados atualizados
                    MessageBox.Show($"Os dados com o código {codigo_cliente} foram alterados com Sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                codigo_cliente = null;

                //Limpa os campos no após o sucesso
                txtNomeCompleto.Text = String.Empty;
                txtNomeSocial.Text = " ";
                txtEmail.Text = " ";
                txtCPF.Text = " ";

                //Recarregar os clientes na Listview
                carregar_clientes();

                //Muda para a aba de pesquisa
                tabControl1.SelectedIndex = 1;
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

            // Restaura o cursor para o padrão
            Cursor = Cursors.Default;
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM dadosdecliente WHERE nomecompleto LIKE @q OR nomesocial LIKE @q ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }

        private void lstCliente_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection clientedaselecao = lstCliente.SelectedItems;

            foreach(ListViewItem item in clientedaselecao)
            {
                codigo_cliente = Convert.ToInt32(item.SubItems[0].Text);

                //Exibe uma messagebox com o codigo do cliente
                MessageBox.Show("Código do cliente: " + codigo_cliente.ToString(),
                    "Código Selecionado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                txtNomeCompleto.Text = item.SubItems[1].Text;
                txtNomeSocial.Text = item.SubItems[2].Text;
                txtEmail.Text = item.SubItems[3].Text;
                txtCPF.Text = item.SubItems[4].Text;
            }

            //Muda para a aba de dados do cliente
            tabControl1.SelectedIndex = 0;
        }

        private void btnNovoCliente_Click(object sender, EventArgs e)
        {
            codigo_cliente = null;

            txtNomeCompleto.Text = String.Empty;
            txtNomeSocial.Text = " ";
            txtEmail.Text = " ";
            txtCPF.Text = " ";

            txtNomeCompleto.Focus();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            excluir_cliente();
        }

        private void btnExcluirCliente_Click(object sender, EventArgs e)
        {
            excluir_cliente();
        }

        private void excluir_cliente()
        {
            try
            {
                DialogResult opcaoDigitada = MessageBox.Show("Tem certeza que deseja excluir o registro de código: " + codigo_cliente,
                    "Tem certeza?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (opcaoDigitada == DialogResult.Yes)
                {
                    Conexao = new MySqlConnection(data_source);

                    Conexao.Open();

                    MySqlCommand cmd = new MySqlCommand();

                    cmd.Connection = Conexao;

                    cmd.Prepare();

                    cmd.CommandText = "DELETE FROM dadosdecliente WHERE codigo = @codigo";

                    cmd.Parameters.AddWithValue("@codigo", codigo_cliente);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Os dados do cliente foram EXCLUÍDOS!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    carregar_clientes();
                }
            }
            catch (MySqlException ex)
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
                if (Conexao != null && Conexao.State == ConnectionState.Open)
                {
                    Conexao.Close();
                    // Teste de Fechamento
                    // MessageBox.Show("Conexão fechada com sucesso");
                }
            }
        }
    }
}
