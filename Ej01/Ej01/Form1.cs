using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Ej01
{
    public partial class Form1 : Form

    {
        // Este es nuestro árbol binario de búsqueda donde guardamos todos los números
        private readonly BinarySearchTree _arbol = new BinarySearchTree();

        public Form1()
        {
            InitializeComponent();
            // Al presionar Enter será como hacer clic en Agregar. Esto agiliza las pruebas.
            this.AcceptButton = btnAgregar;

            // Enlazo por código el filtro de teclado del TextBox
            // Puedes hacerlo también desde el diseñador asignando el evento KeyPress
            txtNumero.KeyPress += txtNumero_KeyPress;

            // Dibujamos el árbol al iniciar para mostrar "(vacío)" si no hay nada
            RenderArbol();

            //hacemos que el boton limpiar funcione
            btnLimpiar.Click += btnLimpiar_Click;

            MessageBox.Show(
            "Este programa utiliza un Árbol Binario de Búsqueda (BST).\n" +
            "Los valores se acomodan automáticamente según su orden:\n" +
            "- Menores van a la izquierda\n" +
            "- Mayores van a la derecha\n\n" +
            "Tú solo escribes un número y el árbol decide dónde colocarlo.",
            "Información"
            );

        }
        // Al presionar teclas dentro del TextBox solo permitimos:
        // dígitos, una tecla '-' al inicio para negativos, teclas de control como Backspace.
        private void txtNumero_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;

            // Permitir teclas de control
            if (char.IsControl(c))
                return;

            // Permitir un solo '-' y únicamente como primer carácter
            if (c == '-')
            {
                if (txtNumero.SelectionStart != 0 || txtNumero.Text.Contains("-"))
                    e.Handled = true; // bloquear
                return;
            }

            // Permitir solo dígitos
            if (!char.IsDigit(c))
                e.Handled = true; // bloquear cualquier otra cosa
        }

        // Botón Agregar: inserta un único número tomado del TextBox
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            string entrada = txtNumero.Text.Trim();

            // Validación 1: no vacío
            if (string.IsNullOrEmpty(entrada))
            {
                MessageBox.Show("Escribe un número entero en el cuadro de texto.");
                txtNumero.Focus();
                return;
            }
            // Validación 2: no permitir separadores ni espacios internos
            // Si el usuario pegó algo con comas, punto y coma o espacios, lo rechazamos
            if (entrada.Contains(",") || entrada.Contains(";") || entrada.Contains(" ") || entrada.Contains("\t") || entrada.Contains("\n"))
            {
                MessageBox.Show("Solo se permite un número. No uses comas, punto y coma ni espacios.");
                txtNumero.Focus();
                return;
            }

            // Validación 3: debe ser entero válido
            if (!int.TryParse(entrada, out int valor))
            {
                MessageBox.Show("Número inválido. Escribe un entero válido, por ejemplo: -5, 0, 12.");
                txtNumero.Focus();
                txtNumero.SelectAll();
                return;
            }

            // Intentamos insertar. Si estaba repetido lo informamos.
            bool insertado = _arbol.Insert(valor);
            if (insertado)
                MessageBox.Show($"Se insertó el número: {valor}");
            else
                MessageBox.Show($"El número {valor} ya existe en el árbol y no se insertó.");

            // Limpiamos y redibujamos
            txtNumero.Clear();
            RenderArbol();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (_arbol.IsEmpty)
            {
                MessageBox.Show("El árbol está vacío. Inserta números primero.");
                return;
            }

            string entrada = Interaction.InputBox("Ingresa el número a buscar:", "Buscar en el árbol", "");

            // Cancelado o vacío
            if (entrada == "")
            {
                MessageBox.Show("Búsqueda cancelada.");
                return;
            }

            // No permitimos separadores aquí tampoco
            if (entrada.Contains(",") || entrada.Contains(";") || entrada.Contains(" ") || entrada.Contains("\t") || entrada.Contains("\n"))
            {
                MessageBox.Show("Escribe un único número, sin comas ni espacios.");
                return;
            }

            // Debe ser entero válido
            if (!int.TryParse(entrada, out int objetivo))
            {
                MessageBox.Show("Por favor escribe un número entero válido.");
                return;
            }

            bool existe = _arbol.Contains(objetivo);
            MessageBox.Show(existe
                ? $"El número {objetivo} SÍ está en el árbol."
                : $"El número {objetivo} NO está en el árbol.");

            if (existe)
                SeleccionarNodo(tvArbol.Nodes, objetivo);
        }

        private void btnMostrar_Click(object sender, EventArgs e)
        {
            if(_arbol.IsEmpty)
            {
                MessageBox.Show("El árbol está vacío.");
                return;
            }

            var lista = _arbol.InOrder();
            var texto = string.Join(", ", lista);
            MessageBox.Show("Recorrido Inorden: " + texto);
        }

        private void btnVacio_Click(object sender, EventArgs e)
        {
            MessageBox.Show(_arbol.IsEmpty
                ? "Sí, el árbol está vacío."
                : "No, el árbol tiene elementos.");
        }

        // Botón Limpiar: borra el TreeView y opcionalmente el árbol
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Preguntamos si desea limpiar completamente el árbol o solo la vista
            var respuesta = MessageBox.Show(
                "¿Deseas borrar también los datos del árbol?",
                "Limpiar vista",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Cancel)
                return;

            if (respuesta == DialogResult.Yes)
            {
                // Si dice que sí, resetear el árbol
                typeof(BinarySearchTree)
                    .GetProperty("Root")
                    .SetValue(_arbol, null);

                MessageBox.Show("El árbol se vació completamente.");
            }

            // Limpia el TreeView visual siempre
            tvArbol.Nodes.Clear();
            tvArbol.Nodes.Add("(vacío)");
        }

        // Dibuja la estructura del árbol en el TreeView
        private void RenderArbol()
        {
            tvArbol.BeginUpdate();
            tvArbol.Nodes.Clear();

            if (_arbol.IsEmpty)
            {
                tvArbol.Nodes.Add(new TreeNode("(vacío)"));
                tvArbol.EndUpdate();
                return;
            }

            var raiz = CrearNodoTreeView(_arbol.Root);
            tvArbol.Nodes.Add(raiz);
            tvArbol.ExpandAll();
            tvArbol.EndUpdate();
        }

        // Convierte un Node del BST en TreeNode recursivamente
        private TreeNode CrearNodoTreeView(Node nodo)
        {
            var tn = new TreeNode(nodo.Value.ToString());

            if (nodo.Left != null) tn.Nodes.Add(CrearNodoTreeView(nodo.Left));
            if (nodo.Right != null) tn.Nodes.Add(CrearNodoTreeView(nodo.Right));

            return tn;
        }

        // Selecciona visualmente un valor en el TreeView si existe
        private void SeleccionarNodo(TreeNodeCollection nodos, int valor)
        {
            foreach (TreeNode n in nodos)
            {
                if (n.Text == valor.ToString())
                {
                    tvArbol.SelectedNode = n;
                    n.EnsureVisible();
                    return;
                }
                SeleccionarNodo(n.Nodes, valor);
            }
        }
    }

    // Lógica del Árbol Binario de Búsqueda

    public class Node
    {
        public int Value;
        public Node Left;
        public Node Right;

        public Node(int value)
        {
            Value = value;
        }
    }

    public class BinarySearchTree
    {
        public Node Root { get; private set; }
        public bool IsEmpty => Root == null;

        // Inserta un entero. Si ya existe, no lo inserta y retorna false.
        public bool Insert(int value)
        {
            if (Root == null)
            {
                Root = new Node(value);
                return true;
            }

            Node actual = Root;

            while (true)
            {
                if (value == actual.Value)
                    return false; // duplicado

                if (value < actual.Value)
                {
                    if (actual.Left == null)
                    {
                        actual.Left = new Node(value);
                        return true;
                    }
                    actual = actual.Left;
                }
                else
                {
                    if (actual.Right == null)
                    {
                        actual.Right = new Node(value);
                        return true;
                    }
                    actual = actual.Right;
                }
            }
        }

        // Busca un valor en el BST
        public bool Contains(int value)
        {
            Node actual = Root;

            while (actual != null)
            {
                if (value == actual.Value) return true;
                actual = value < actual.Value ? actual.Left : actual.Right;
            }

            return false;
        }

        // Recorrido inorden: izquierda, raíz, derecha
        public List<int> InOrder()
        {
            var res = new List<int>();
            InOrder(Root, res);
            return res;
        }

        private void InOrder(Node n, List<int> acc)
        {
            if (n == null) return;

            InOrder(n.Left, acc);
            acc.Add(n.Value);
            InOrder(n.Right, acc);
        }
    }
}
