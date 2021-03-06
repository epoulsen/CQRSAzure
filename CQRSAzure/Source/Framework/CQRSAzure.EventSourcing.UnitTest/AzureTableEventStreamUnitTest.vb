﻿Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports CQRSAzure.EventSourcing
Imports CQRSAzure.EventSourcing.Azure.Table
Imports CQRSAzure.EventSourcing.UnitTest.Mocking

<TestClass()>
Public Class AzureTableEventStreamUnitTest


    Public Const TEST_AGGREGATE_IDENTIFIER As String = "TABLE.OK.331.987"

    <TestMethod()>
    Public Sub Reader_Constructor_TestMethod()

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As IEventStreamReader(Of MockAggregate, String) = TableEventStreamReader(Of MockAggregate, String).Create(testAgg)
        Assert.IsNotNull(testObj)

    End Sub

    <TestMethod()>
    Public Sub Reader_TableName_TestMethod()

        Dim expected As String = "MockAggregate"
        Dim actual As String = "not set"

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As TableEventStreamReader(Of MockAggregate, String) = TableEventStreamReader(Of MockAggregate, String).Create(testAgg)

        actual = testObj.TableName

        Assert.AreEqual(expected, actual)

    End Sub

    ''' <summary>
    ''' Too short a name, append DATA to it
    ''' </summary>
    <TestMethod>
    Public Sub MakeValidStorageFolderName_TooShort_TestMethod()

        Dim expected As String = "aDATA"
        Dim actual As String = ""

        actual = TableEventStreamBase.MakeValidStorageTableName("-a")

        Assert.AreEqual(expected, actual)

    End Sub

    ''' <summary>
    ''' Too short a name, append DATA to it
    ''' </summary>
    <TestMethod>
    Public Sub MakeValidStorageTableName_TooShortAfterInvalid_TestMethod()

        Dim expected As String = "aDATA"
        Dim actual As String = ""

        actual = TableEventStreamBase.MakeValidStorageTableName("-... -!a")

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod>
    Public Sub MakeValidStorageTableName_FixChars_TestMethod()

        Dim expected As String = "DuncansModel"
        Dim actual As String = ""

        actual = TableEventStreamBase.MakeValidStorageTableName("Duncan's Model")

        Assert.AreEqual(expected, actual)

    End Sub


    <TestMethod()>
    Public Sub Writer_WriteEvent_TestMethod()

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As TableEventStreamWriter(Of MockAggregate, String) = TableEventStreamWriter(Of MockAggregate, String).Create(testAgg)
        testObj.AppendEvent(New MockEventTypeOne() With {.EventOneStringProperty = "My test", .EventOneIntegerProperty = 123})

        Assert.IsTrue(testObj.RecordCount > 0)

    End Sub

    <TestMethod()>
    Public Sub Writer_WriteMultipleEvent_TestMethod()

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As TableEventStreamWriter(Of MockAggregate, String) = TableEventStreamWriter(Of MockAggregate, String).Create(testAgg)
        testObj.SetContext(New MockContext("First event", "Unit testing", "Duncan"))
        testObj.AppendEvent(New MockEventTypeOne() With {.EventOneStringProperty = "My test 2", .EventOneIntegerProperty = 132})
        testObj.AppendEvent(New MockEventTypeOne() With {.EventOneStringProperty = "My test 3", .EventOneIntegerProperty = 133})
        testObj.AppendEvent(New MockEventTypeOne() With {.EventOneStringProperty = "My test 4", .EventOneIntegerProperty = 134})
        testObj.SetContext(New MockContext("Last event", "Unit testing", "Christina"))
        testObj.AppendEvent(New MockEventTypeOne() With {.EventOneStringProperty = "My test 5", .EventOneIntegerProperty = 135})

        Assert.IsTrue(testObj.RecordCount > 3)

    End Sub

    <TestMethod()>
    Public Sub Reader_ReadEvents_TestMethod()

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As TableEventStreamReader(Of MockAggregate, String) = TableEventStreamReader(Of MockAggregate, String).Create(testAgg)
        Dim ret = testObj.GetEvents()

        Assert.IsNotNull(ret)

    End Sub


    <TestMethod()>
    Public Sub Reader_ReadEventsWithContext_TestMethod()

        Dim testAgg As New MockAggregate(TEST_AGGREGATE_IDENTIFIER)
        Dim testObj As TableEventStreamReader(Of MockAggregate, String) = TableEventStreamReader(Of MockAggregate, String).Create(testAgg)
        Dim ret = testObj.GetEventsWithContext()

        Assert.IsNotNull(ret)

    End Sub


    <TestMethod()>
    Public Sub GetAllStreamKeys_NoDate_TestMethod()

        Dim expected As Boolean = True
        Dim actual As Boolean = False

        Dim testObj = TableEventStreamReader(Of MockAggregate, String).Create(New MockAggregate(TEST_AGGREGATE_IDENTIFIER))
        Dim testProvider As IEventStreamProvider(Of MockAggregate, String) = TableEventStreamProviderFactory.Create(Of MockAggregate, String)()

        actual = testProvider.GetAllStreamKeys().Contains(TEST_AGGREGATE_IDENTIFIER)

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub GetAllStreamKeys_GUIDKey_NoDate_TestMethod()

        Dim expected As Boolean = True
        Dim actual As Boolean = False

        Dim key As Guid = Guid.NewGuid()

        Dim testwriter As TableEventStreamWriter(Of MockGuidAggregate, Guid) = TableEventStreamWriter(Of MockGuidAggregate, Guid).Create(New MockGuidAggregate(key))
        testwriter.Reset()
        testwriter.AppendEvent(New MockGuidEventTypeTwo() With {.EventTwoNullableDateProperty = DateTime.UtcNow})

        Dim testProvider = TableEventStreamProviderFactory.Create(Of MockGuidAggregate, Guid)()

        actual = testProvider.GetAllStreamKeys().Contains(key)

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub GetAllStreamKeys_GUIDKey_FutureDate_TestMethod()

        Dim expected As Boolean = False
        Dim actual As Boolean = True

        Dim asOfdate As DateTime = New DateTime(2037, 1, 1)

        Dim key As Guid = Guid.NewGuid()

        Dim testObj = TableEventStreamWriter(Of MockGuidAggregate, Guid).Create(New MockGuidAggregate(key))
        Dim testProvider As IEventStreamProvider(Of MockGuidAggregate, Guid) = TableEventStreamProviderFactory.Create(Of MockGuidAggregate, Guid)()

        actual = testProvider.GetAllStreamKeys(asOfdate).Contains(key)

        Assert.AreEqual(expected, actual)

    End Sub
End Class

<TestClass>
Public Class CQRSAzureEventSourcingTableSettingsElementUnitTest

    <TestMethod()>
    Public Sub IsValidCustomNumberFormat_Valid_TestMethod()

        Dim expected As String = "00000"
        Dim actual As String = "not set"

        Dim testObj As New CQRSAzureEventSourcingTableSettingsElement()
        testObj.SequenceNumberFormat = expected
        actual = testObj.SequenceNumberFormat

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub IsValidCustomNumberFormat_InValid_TestMethod()

        Dim expected As String = CQRSAzureEventSourcingTableSettingsElement.DEFAULT_SEQUENCENUMBERFORMAT
        Dim actual As String = "not set"

        Dim testObj As New CQRSAzureEventSourcingTableSettingsElement()
        testObj.SequenceNumberFormat = "invalid"
        actual = testObj.SequenceNumberFormat

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub IsValidCustomNumberFormat_Blank_TestMethod()

        Dim expected As String = CQRSAzureEventSourcingTableSettingsElement.DEFAULT_SEQUENCENUMBERFORMAT
        Dim actual As String = "not set"

        Dim testObj As New CQRSAzureEventSourcingTableSettingsElement()
        testObj.SequenceNumberFormat = "invalid"
        actual = testObj.SequenceNumberFormat

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub IsValidCustomNumberFormat_Hex_TestMethod()

        Dim expected As String = "X"
        Dim actual As String = "not set"

        Dim testObj As New CQRSAzureEventSourcingTableSettingsElement()
        testObj.SequenceNumberFormat = expected
        actual = testObj.SequenceNumberFormat

        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()>
    Public Sub IsValidCustomNumberFormat_General_TestMethod()

        Dim expected As String = CQRSAzureEventSourcingTableSettingsElement.DEFAULT_SEQUENCENUMBERFORMAT
        Dim actual As String = "not set"

        Dim testObj As New CQRSAzureEventSourcingTableSettingsElement()
        testObj.SequenceNumberFormat = "G"
        actual = testObj.SequenceNumberFormat

        Assert.AreEqual(expected, actual)

    End Sub

End Class