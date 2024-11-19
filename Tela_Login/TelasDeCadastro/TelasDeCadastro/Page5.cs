﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tela_Login;

namespace TelasDeCadastro
{
    public partial class Page5 : Form
    {
        public Page5()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
        }

  

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {

            // Cria uma nova instância do formulário de login
            Form1 loginForm = new Form1();

            // Exibe o formulário de login
            loginForm.Show();

            // Oculta o formulário atual
            this.Hide();

        }
    }
}
