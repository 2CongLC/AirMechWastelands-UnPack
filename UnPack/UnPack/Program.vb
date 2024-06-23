Imports System
Imports System.Text
Imports System.IO
Imports System.IO.Compression


Module Program

    Private br As BinaryReader
    Private des As String
    Private source As String
    Private buffer As Byte()
    Private ms As MemoryStream

    Sub Main(args As String())

        If args.Count = 0 Then
            Console.WriteLine("UnPack Tool - 2CongLC.vn")
        Else
            source = args(0)
        End If

        If File.Exists(source) Then

            br = New BinaryReader(File.OpenRead(source))
            Dim unknow As Int32 = br.ReadInt32
            Dim count As Int32 = br.ReadInt32
            Dim offset As Int32 = br.ReadInt32
            Dim unknow1 As Int32 = br.ReadInt32
            Dim unknow2 As Int32 = br.ReadInt32
            Dim sizeName As Int32 = br.ReadInt32
            Dim unknow3 As Int16 = br.ReadInt16

            Using dfs As New DeflateStream(New MemoryStream(br.ReadBytes(sizeName - 2)), CompressionMode.Decompress)
                dfs.CopyTo(ms)
            End Using

            Dim subfiles As New List(Of FileData)()
            For i As Int32 = 0 To count - 1
                subfiles.Add(New FileData)
            Next

            br.BaseStream.Position = offset

            des = Path.GetDirectoryName(source) + "\" + Path.GetFileNameWithoutExtension(source)

            For Each fd As FileData In subfiles
                Console.WriteLine("File Offset : {0} - File SizeCompressed : {1} - File SizeUncompressed : {2} - File Name : {3}", fd.offset, fd.sizeCompressed, fd.sizeUncompressed, fd.name)
                br.BaseStream.Position = fd.offset
                Dim buffer As Byte()
                Directory.CreateDirectory(des + "\" + Path.GetDirectoryName(fd.name))
                Dim fs As FileStream = File.Create(des + "\" + fd.name)
                If fd.sizeCompressed = fd.sizeUncompressed Then
                    buffer = br.ReadBytes(fd.sizeUncompressed)
                    Using bw As New BinaryWriter(fs)
                        bw.Write(buffer)
                    End Using
                Else
                    buffer = br.ReadBytes(fd.sizeCompressed - 2)
                    Dim unknow4 As Int16 = br.ReadInt16
                    Using dfs As New DeflateStream(New MemoryStream(buffer), CompressionMode.Decompress)
                        dfs.CopyTo(fs)
                    End Using
                End If
                fs.Close()

            Next
            br.Close()
            Console.WriteLine("Unpack Done !!!")
        End If

        Console.ReadLine()
    End Sub

    Class FileData
        Public sizeCompressed As Int32 = br.ReadInt32()
        Public offset As Int32 = br.ReadInt32()
        Public sizeUncompressed As Int32 = br.ReadInt32()
        Public unknown As Single = br.ReadSingle()
        Public name As String = getname(br.ReadInt32)
    End Class

    Private Function getname(ByVal offset As Int32) As String
        ms.Position = offset
        Dim _name As String = ""
        Dim x As Byte = CByte(ms.ReadByte)
        While x <> 0
            _name &= ChrW(x)
            x = CByte(ms.ReadByte)
        End While
        Return _name
    End Function

End Module
