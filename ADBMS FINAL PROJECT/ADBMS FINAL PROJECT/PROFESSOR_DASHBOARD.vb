﻿Imports Microsoft.Office.Interop
Imports System.Data.OleDb
Imports System.IO

Public Class PROFESSOR_DASHBOARD
    ' Access database connection string - update path to your register.accdb file
    Private connectionString As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=register.accdb;Persist Security Info=False;"
    Private studentsToImport As New List(Of Student)()

    Private Class Student
        Public Property SchoolYear As String
        Public Property Course As String
        Public Property Section As String
        Public Property SubjectCode As String
        Public Property SubjectTitle As String
        Public Property Semester As String
        Public Property Instructor As String
        Public Property GradeID As String
        Public Property StudentID As String
        Public Property Name As String
        Public Property GPA As String
        Public Property Remarks As String
    End Class

    Private Class ExcelRow
        Public Property StudentID As String
        Public Property Name As String
        Public Property GPA As String
        Public Property Remarks As String

        Public Sub New()
            StudentID = ""
            Name = ""
            GPA = ""
            Remarks = ""
        End Sub
    End Class

    Private Sub PROFESSOR_DASHBOARD_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialize form components
        LoadProfessorData()

        ' Set up OpenFileDialog for Excel import
        OpenFileDialog1.Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*"
        OpenFileDialog1.DefaultExt = "xlsx"
        OpenFileDialog1.Title = "Import Grade Sheet"

        ' Enable checkboxes in ListView
        lvprofessor.CheckBoxes = True

        ' Make remarks field read-only
        txtstudremarks.ReadOnly = True
        txtstudremarks.BackColor = Color.LightGray
    End Sub

    Private Sub LoadProfessorData()
        ' Load existing professor data into ListView
        Try
            Using connection As New OleDbConnection(connectionString)
                connection.Open()
                Dim query As String = "SELECT * FROM tbl_professor ORDER BY StudentID DESC"
                Dim command As New OleDbCommand(query, connection)
                Dim reader As OleDbDataReader = command.ExecuteReader()

                lvprofessor.Items.Clear()
                studentsToImport.Clear()

                While reader.Read()
                    Dim student As New Student With {
                        .SchoolYear = If(reader("SchoolYear") IsNot DBNull.Value, reader("SchoolYear").ToString(), ""),
                        .Course = If(reader("Course") IsNot DBNull.Value, reader("Course").ToString(), ""),
                        .Section = If(reader("Section") IsNot DBNull.Value, reader("Section").ToString(), ""),
                        .SubjectCode = If(reader("SubjectCode") IsNot DBNull.Value, reader("SubjectCode").ToString(), ""),
                        .SubjectTitle = If(reader("SubjectTitle") IsNot DBNull.Value, reader("SubjectTitle").ToString(), ""),
                        .Semester = If(reader("Semester") IsNot DBNull.Value, reader("Semester").ToString(), ""),
                        .Instructor = If(reader("Instructor") IsNot DBNull.Value, reader("Instructor").ToString(), ""),
                        .GradeID = If(reader("GradeID") IsNot DBNull.Value, reader("GradeID").ToString(), ""),
                        .StudentID = If(reader("StudentID") IsNot DBNull.Value, reader("StudentID").ToString(), ""),
                        .Name = If(reader("StudentName") IsNot DBNull.Value, reader("StudentName").ToString(), ""),
                        .GPA = If(reader("GPA") IsNot DBNull.Value, reader("GPA").ToString(), ""),
                        .Remarks = If(reader("Remarks") IsNot DBNull.Value, reader("Remarks").ToString(), "")
                    }

                    ' Ensure remarks are set based on GPA if empty
                    If String.IsNullOrWhiteSpace(student.Remarks) Then
                        Dim gpaDouble As Double
                        If Double.TryParse(student.GPA, gpaDouble) Then
                            student.Remarks = If(gpaDouble >= 3, "PASSED", "FAILED")
                        Else
                            student.Remarks = "FAILED"
                        End If
                    End If

                    studentsToImport.Add(student)

                    Dim lvi As New ListViewItem(student.StudentID)
                    lvi.SubItems.Add(student.Name)
                    lvi.SubItems.Add(student.GPA)
                    lvi.SubItems.Add(student.Remarks)
                    lvi.Tag = student
                    lvprofessor.Items.Add(lvi)
                End While

                reader.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading professor data: " & ex.Message)
        End Try
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        ImportFromExcel(OpenFileDialog1.FileName)
    End Sub

    Private Sub ImportFromExcel(filePath As String)
        Dim xlApp As Excel.Application = Nothing
        Dim xlWorkbook As Excel.Workbook = Nothing
        Dim xlWorksheet As Excel.Worksheet = Nothing

        Try
            xlApp = New Excel.Application()
            xlWorkbook = xlApp.Workbooks.Open(filePath)
            xlWorksheet = DirectCast(xlWorkbook.Sheets(1), Excel.Worksheet)

            ' Clear existing items
            lvprofessor.Items.Clear()
            studentsToImport.Clear()

            ' Read header information
            txtstudsy.Text = If(xlWorksheet.Cells(1, 2).Value2, "").ToString()
            cbostudcourse.Text = If(xlWorksheet.Cells(2, 2).Value2, "").ToString()
            txtstudsection.Text = If(xlWorksheet.Cells(3, 2).Value2, "").ToString()
            txtsubjcode.Text = If(xlWorksheet.Cells(4, 2).Value2, "").ToString()
            txtsubjtitle.Text = If(xlWorksheet.Cells(5, 2).Value2, "").ToString()
            txtstudsem.Text = If(xlWorksheet.Cells(6, 2).Value2, "").ToString()
            txtstudprof.Text = If(xlWorksheet.Cells(1, 4).Value2, "").ToString()
            txtgradeid.Text = If(xlWorksheet.Cells(2, 4).Value2, "").ToString()

            ' Start from row 9 (skip header in row 8)
            Dim row As Integer = 9

            ' Read student data
            While True
                ' Get student ID
                Dim studentID As String = ""
                If xlWorksheet.Cells(row, 1).Value2 IsNot Nothing Then
                    studentID = xlWorksheet.Cells(row, 1).Value2.ToString()
                Else
                    Exit While
                End If

                ' Get other data
                Dim schoolYear As String = If(txtstudsy.Text, "").ToString()
                Dim course As String = If(cbostudcourse.Text, "").ToString()
                Dim section As String = If(txtstudsection.Text, "").ToString()
                Dim subjectCode As String = If(txtsubjcode.Text, "").ToString()
                Dim subjectTitle As String = If(txtsubjtitle.Text, "").ToString()
                Dim semester As String = If(txtstudsem.Text, "").ToString()
                Dim instructor As String = If(txtstudprof.Text, "").ToString()
                Dim gradeID As String = If(txtgradeid.Text, "").ToString()
                Dim studentName As String = If(xlWorksheet.Cells(row, 2).Value2, "").ToString()
                Dim gpa As String = If(xlWorksheet.Cells(row, 3).Value2, "").ToString()

                ' Calculate remarks based on GPA
                Dim remarks As String
                Dim gpaDouble As Double
                If Double.TryParse(gpa, gpaDouble) Then
                    remarks = If(gpaDouble >= 3, "PASSED", "FAILED")
                Else
                    remarks = "FAILED"
                End If

                ' Create student object
                Dim student As New Student With {
                    .SchoolYear = schoolYear,
                    .Course = course,
                    .Section = section,
                    .SubjectCode = subjectCode,
                    .SubjectTitle = subjectTitle,
                    .Semester = semester,
                    .Instructor = instructor,
                    .GradeID = gradeID,
                    .StudentID = studentID,
                    .Name = studentName,
                    .GPA = gpa,
                    .Remarks = remarks
                }

                ' Add to collection
                studentsToImport.Add(student)

                ' Add to ListView
                Dim lvi As New ListViewItem(studentID)
                lvi.SubItems.Add(studentName)
                lvi.SubItems.Add(gpa)
                lvi.SubItems.Add(remarks)
                lvi.Tag = student
                lvprofessor.Items.Add(lvi)

                row += 1
            End While

            MessageBox.Show($"Loaded {studentsToImport.Count} records. Select records and click Save.")

        Catch ex As Exception
            MessageBox.Show("Error loading Excel file: " & ex.Message)
        Finally
            If xlWorksheet IsNot Nothing Then
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet)
            End If
            If xlWorkbook IsNot Nothing Then
                xlWorkbook.Close(False)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook)
            End If
            If xlApp IsNot Nothing Then
                xlApp.Quit()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp)
            End If

            xlWorksheet = Nothing
            xlWorkbook = Nothing
            xlApp = Nothing
            GC.Collect()
            GC.WaitForPendingFinalizers()
        End Try
    End Sub

    Private Sub btnprofbatch_Click(sender As Object, e As EventArgs) Handles btnprofbatch.Click
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            ImportFromExcel(OpenFileDialog1.FileName)
        End If
    End Sub

    Private Sub btnprofsave_Click(sender As Object, e As EventArgs) Handles btnprofsave.Click
        If studentsToImport.Count > 0 AndAlso lvprofessor.CheckedItems.Count > 0 Then
            ' Debug - show counts
            MessageBox.Show($"Total items: {studentsToImport.Count}{vbCrLf}Selected items: {lvprofessor.CheckedItems.Count}")

            If MessageBox.Show($"Save {lvprofessor.CheckedItems.Count} selected records?", "Confirm", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                SaveSelectedStudents()
            End If
        Else
            SaveProfessorData()
        End If
    End Sub

    Private Sub SaveSelectedStudents()
        If lvprofessor.CheckedItems.Count = 0 Then
            MessageBox.Show("Please select records to save by checking the checkboxes.")
            Return
        End If

        ' Debug message to show selected items
        Dim selectedCount As Integer = lvprofessor.CheckedItems.Count
        MessageBox.Show($"Selected {selectedCount} items to save")

        ' Validate header info
        If String.IsNullOrWhiteSpace(txtstudsy.Text) OrElse
           String.IsNullOrWhiteSpace(cbostudcourse.Text) OrElse
           String.IsNullOrWhiteSpace(txtstudsection.Text) Then
            MessageBox.Show("Please fill in School Year, Course, and Section.")
            Return
        End If

        Dim connection As OleDbConnection = Nothing
        Dim saved As Integer = 0
        Dim errors As New List(Of String)

        Try
            connection = New OleDbConnection(connectionString)
            connection.Open()

            ' Use transaction for atomicity
            Using transaction = connection.BeginTransaction()
                ' Save each selected student
                For Each item As ListViewItem In lvprofessor.CheckedItems
                    Try
                        Dim student As Student = DirectCast(item.Tag, Student)

                        ' Calculate Remarks based on GPA
                        Dim gpaDouble As Double
                        If Double.TryParse(student.GPA, gpaDouble) Then
                            student.Remarks = If(gpaDouble >= 3, "PASSED", "FAILED")
                        Else
                            student.Remarks = "FAILED"
                        End If

                        ' Check for duplicate StudentID before insert
                        Using checkCmd As New OleDbCommand("SELECT COUNT(*) FROM tbl_professor WHERE StudentID = ?", connection, transaction)
                            checkCmd.Parameters.AddWithValue("", student.StudentID.Trim())
                            Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                            If count > 0 Then
                                errors.Add($"Duplicate StudentID found: {student.StudentID}. Skipping insert.")
                                Continue For
                            End If
                        End Using

                        Using cmd As New OleDbCommand()
                            cmd.Connection = connection
                            cmd.Transaction = transaction
                            cmd.CommandText = "INSERT INTO [tbl_professor] " &
                                              "([SchoolYear], [Course], [Section], [SubjectCode], [SubjectTitle], " &
                                              "[Semester], [Instructor], [GradeID], [StudentID], [StudentName], [GPA], [Remarks]) " &
                                              "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"

                            ' Add parameters
                            With cmd.Parameters
                                .Add("@sy", OleDbType.VarChar).Value = student.SchoolYear.Trim()
                                .Add("@course", OleDbType.VarChar).Value = student.Course.Trim()
                                .Add("@section", OleDbType.VarChar).Value = student.Section.Trim()
                                .Add("@subjcode", OleDbType.VarChar).Value = student.SubjectCode.Trim()
                                .Add("@subjtitle", OleDbType.VarChar).Value = student.SubjectTitle.Trim()
                                .Add("@sem", OleDbType.VarChar).Value = student.Semester.Trim()
                                .Add("@instructor", OleDbType.VarChar).Value = student.Instructor.Trim()
                                .Add("@gradeid", OleDbType.VarChar).Value = student.GradeID.Trim()
                                .Add("@studentid", OleDbType.VarChar).Value = student.StudentID.Trim()
                                .Add("@studentname", OleDbType.VarChar).Value = student.Name.Trim()

                                ' Handle GPA
                                Dim gpaParam = .Add("@gpa", OleDbType.Double)
                                If String.IsNullOrWhiteSpace(student.GPA) Then
                                    gpaParam.Value = DBNull.Value
                                Else
                                    If Double.TryParse(student.GPA.Trim(), gpaDouble) Then
                                        gpaParam.Value = gpaDouble
                                    Else
                                        gpaParam.Value = DBNull.Value
                                    End If
                                End If

                                ' Handle Remarks (always calculated from GPA)
                                .Add("@remarks", OleDbType.VarChar).Value = student.Remarks
                            End With

                            Try
                                cmd.ExecuteNonQuery()
                                saved += 1
                            Catch ex As Exception
                                errors.Add($"Error saving {student.StudentID}: {ex.Message}")
                            End Try
                        End Using
                    Catch ex As Exception
                        errors.Add($"Error processing student {item.Text}: {ex.Message}")
                    End Try
                Next

                ' Commit transaction if no errors
                If errors.Count = 0 Then
                    transaction.Commit()
                Else
                    transaction.Rollback()
                End If
            End Using

            ' Show results
            Dim message As String = $"Processed {selectedCount} records.{vbCrLf}Successfully saved: {saved} records"
            If errors.Count > 0 Then
                message &= vbCrLf & vbCrLf & "Errors:" & vbCrLf
                message &= String.Join(vbCrLf, errors)
            End If

            MessageBox.Show(message)

            ' If we saved any records, refresh the view
            If saved > 0 Then
                lvprofessor.Items.Clear()
                studentsToImport.Clear()
                LoadProfessorData()
            End If

        Catch ex As Exception
            MessageBox.Show("Database error: " & ex.Message)
        Finally
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then
                connection.Close()
            End If
        End Try
    End Sub

    Private Sub SaveProfessorData()
        ' Calculate remarks based on GPA
        Dim remarks As String = "FAILED"
        Dim gpaDouble As Double
        If Double.TryParse(txtstudgpa.Text, gpaDouble) Then
            remarks = If(gpaDouble >= 3, "PASSED", "FAILED")
        End If
        txtstudremarks.Text = remarks

        ' Declare the query variable at the method level
        Dim query As String = "INSERT INTO tbl_professor ([SchoolYear], [Course], [Section], [SubjectCode], " &
                        "[SubjectTitle], [Semester], [Instructor], [GradeID], [StudentID], [StudentName], " &
                        "[GPA], [Remarks]) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"

        Try
            Using connection As New OleDbConnection(connectionString)
                connection.Open()
                Dim command As New OleDbCommand(query, connection)

                ' Add parameters in EXACT order of columns
                command.Parameters.AddWithValue("", txtstudsy.Text)
                command.Parameters.AddWithValue("", cbostudcourse.Text)
                command.Parameters.AddWithValue("", txtstudsection.Text)
                command.Parameters.AddWithValue("", txtsubjcode.Text)
                command.Parameters.AddWithValue("", txtsubjtitle.Text)
                command.Parameters.AddWithValue("", txtstudsem.Text)
                command.Parameters.AddWithValue("", txtstudprof.Text)
                command.Parameters.AddWithValue("", txtgradeid.Text)
                command.Parameters.AddWithValue("", txtstudid.Text)
                command.Parameters.AddWithValue("", txtstudname.Text)

                ' Handle GPA
                If String.IsNullOrWhiteSpace(txtstudgpa.Text) Then
                    command.Parameters.AddWithValue("", DBNull.Value)
                Else
                    If Double.TryParse(txtstudgpa.Text.Trim(), gpaDouble) Then
                        command.Parameters.AddWithValue("", gpaDouble)
                    Else
                        command.Parameters.AddWithValue("", DBNull.Value)
                    End If
                End If

                ' Add remarks (already calculated)
                command.Parameters.AddWithValue("", remarks)

                command.ExecuteNonQuery()
                MessageBox.Show("Data saved successfully!")
                ClearFormFields()
                LoadProfessorData()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error saving data: " & ex.Message & vbCrLf & "Full query: " & query,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClearFormFields()
        txtstudid.Clear()
        txtstudname.Clear()
        txtstudgpa.Clear()
        txtstudremarks.Clear()
        ' Add other fields as needed
    End Sub

    Private Sub btnprofupdate_Click(sender As Object, e As EventArgs) Handles btnprofupdate.Click
        UpdateProfessorData()
    End Sub

    Private Sub UpdateProfessorData()
        ' Calculate remarks based on GPA
        Dim remarks As String = "FAILED"
        Dim gpaDouble As Double
        If Double.TryParse(txtstudgpa.Text, gpaDouble) Then
            remarks = If(gpaDouble >= 3, "PASSED", "FAILED")
        End If
        txtstudremarks.Text = remarks

        Try
            Using connection As New OleDbConnection(connectionString)
                connection.Open()
                Dim query As String = "UPDATE [tbl_professor] SET [SchoolYear]=?, [Course]=?, [Section]=?, " &
                                    "[SubjectCode]=?, [SubjectTitle]=?, [Semester]=?, [Instructor]=?, [GradeID]=?, " &
                                    "[StudentName]=?, [GPA]=?, [Remarks]=? WHERE [StudentID]=?"

                Dim command As New OleDbCommand(query, connection)
                command.Parameters.AddWithValue("@sy", txtstudsy.Text)
                command.Parameters.AddWithValue("@course", cbostudcourse.Text)
                command.Parameters.AddWithValue("@section", txtstudsection.Text)
                command.Parameters.AddWithValue("@subjcode", txtsubjcode.Text)
                command.Parameters.AddWithValue("@subjtitle", txtsubjtitle.Text)
                command.Parameters.AddWithValue("@semester", txtstudsem.Text)
                command.Parameters.AddWithValue("@instructor", txtstudprof.Text)
                command.Parameters.AddWithValue("@gradeid", txtgradeid.Text)
                command.Parameters.AddWithValue("@studentname", txtstudname.Text)

                ' Handle GPA
                Dim gpaValue As Object = DBNull.Value
                If Not String.IsNullOrWhiteSpace(txtstudgpa.Text) Then
                    If Double.TryParse(txtstudgpa.Text.Trim(), gpaDouble) Then
                        gpaValue = gpaDouble
                    End If
                End If
                command.Parameters.AddWithValue("@gpa", gpaValue)

                ' Add remarks (already calculated)
                command.Parameters.AddWithValue("@remarks", remarks)

                command.Parameters.AddWithValue("@studentid", txtstudid.Text)

                Dim rowsAffected As Integer = command.ExecuteNonQuery()
                If rowsAffected > 0 Then
                    MessageBox.Show("Data updated successfully!")
                    LoadProfessorData()
                Else
                    MessageBox.Show("No record found to update.")
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error updating data: " & ex.Message)
        End Try
    End Sub

    Private Sub btnprofdel_Click(sender As Object, e As EventArgs) Handles btnprofdel.Click
        DeleteProfessorData()
    End Sub

    Private Sub DeleteProfessorData()
        If MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Using connection As New OleDbConnection(connectionString)
                    connection.Open()
                    Dim query As String = "DELETE FROM tbl_professor WHERE StudentID = ?"
                    Dim command As New OleDbCommand(query, connection)
                    command.Parameters.AddWithValue("@studentid", txtstudid.Text)

                    Dim rowsAffected As Integer = command.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        MessageBox.Show("Record deleted successfully!")
                        ClearFormFields()
                        LoadProfessorData()
                    Else
                        MessageBox.Show("No record found to delete.")
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Error deleting record: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub btnprofback_Click(sender As Object, e As EventArgs) Handles btnprofback.Click
        Me.Hide()
        Dim frm1 As New Form1()
        frm1.Show()
    End Sub

    Private Sub btnprofexit_Click(sender As Object, e As EventArgs) Handles btnprofexit.Click
        Application.Exit()
    End Sub

    Private Sub lvprofessor_DoubleClick(sender As Object, e As EventArgs) Handles lvprofessor.DoubleClick
        ' Handle double-click on ListView items
        If lvprofessor.SelectedItems.Count > 0 Then
            Dim selectedStudent As Student = CType(lvprofessor.SelectedItems(0).Tag, Student)
            ' Display the selected student's details in form controls
            txtstudsy.Text = selectedStudent.SchoolYear
            cbostudcourse.Text = selectedStudent.Course
            txtstudsection.Text = selectedStudent.Section
            txtsubjcode.Text = selectedStudent.SubjectCode
            txtsubjtitle.Text = selectedStudent.SubjectTitle
            txtstudsem.Text = selectedStudent.Semester
            txtstudprof.Text = selectedStudent.Instructor
            txtgradeid.Text = selectedStudent.GradeID
            txtstudid.Text = selectedStudent.StudentID
            txtstudname.Text = selectedStudent.Name
            txtstudgpa.Text = selectedStudent.GPA
            txtstudremarks.Text = selectedStudent.Remarks
        End If
    End Sub

    Private Sub txtstudgpa_TextChanged(sender As Object, e As EventArgs) Handles txtstudgpa.TextChanged
        ' Automatically update remarks when GPA changes
        Dim gpaDouble As Double
        If Double.TryParse(txtstudgpa.Text, gpaDouble) Then
            txtstudremarks.Text = If(gpaDouble >= 3, "PASSED", "FAILED")
        Else
            txtstudremarks.Text = "FAILED"
        End If
    End Sub

    ' Other event handlers that don't need implementation can be removed
    Private Sub txtgradeid_TextChanged(sender As Object, e As EventArgs) Handles txtgradeid.TextChanged
    End Sub
    Private Sub txtstudid_TextChanged(sender As Object, e As EventArgs) Handles txtstudid.TextChanged
    End Sub
    Private Sub txtstudname_TextChanged(sender As Object, e As EventArgs) Handles txtstudname.TextChanged
    End Sub
    Private Sub txtstudremarks_TextChanged(sender As Object, e As EventArgs) Handles txtstudremarks.TextChanged
    End Sub
    Private Sub txtstudsy_TextChanged(sender As Object, e As EventArgs) Handles txtstudsy.TextChanged
    End Sub
    Private Sub cbostudcourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbostudcourse.SelectedIndexChanged
    End Sub
    Private Sub txtstudsection_TextChanged(sender As Object, e As EventArgs) Handles txtstudsection.TextChanged
    End Sub
    Private Sub txtsubjcode_TextChanged(sender As Object, e As EventArgs) Handles txtsubjcode.TextChanged
    End Sub
    Private Sub txtsubjtitle_TextChanged(sender As Object, e As EventArgs) Handles txtsubjtitle.TextChanged
    End Sub
    Private Sub txtstudsem_TextChanged(sender As Object, e As EventArgs) Handles txtstudsem.TextChanged
    End Sub
    Private Sub txtstudprof_TextChanged(sender As Object, e As EventArgs) Handles txtstudprof.TextChanged
    End Sub

    ' Add Select All button click handler
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        ToggleAllItems()
    End Sub

    ' Toggle selection of all items
    Private Sub ToggleAllItems()
        ' Get the current state of the first item (if any) to determine the new state
        Dim newState As Boolean = True
        If lvprofessor.Items.Count > 0 Then
            newState = Not lvprofessor.Items(0).Checked
        End If

        ' Apply the new state to all items
        For Each item As ListViewItem In lvprofessor.Items
            item.Checked = newState
        Next

        ' Update button text
        btnSelectAll.Text = If(newState, "Deselect All", "Select All")
    End Sub
End Class