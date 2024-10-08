﻿// using Moq;
// using Rsb.Accessories;
// using Rsb.Core.Messaging;
//
// namespace Rsb.Tests.Core.Messaging;
//
// [TestFixture]
// public class MessagingContextInternalTests
// {
//     private MessagingContextInternal _context;
//
//     [SetUp]
//     public void SetUp()
//     {
//         _context = new MessagingContextInternal();
//     }
//
//     [Test]
//     public void Constructor_InitializesWithEmptyQueueAndNewCorrelationId()
//     {
//         // Assert
//         Assert.Multiple(() =>
//         {
//             Assert.That(_context.Messages, Is.Empty);
//             Assert.That(_context.CorrelationId, Is.Not.EqualTo(Guid.Empty));
//         });
//     }
//
//     [Test]
//     public async Task Send_GivenACommand_EnqueuesMessage()
//     {
//         // Arrange
//         var command = new Mock<IAmACommand>().Object;
//
//         // Act
//         await _context.Send(command);
//
//         // Assert
//         Assert.Multiple(() =>
//         {
//             Assert.That(_context.Messages, Has.Count.EqualTo(1));
//         });
//     }
//
//     [Test]
//     public async Task Publish_GivenAnEvent_EnqueuesMessage()
//     {
//         // Arrange
//         var eventMessage = new Mock<IAmAnEvent>().Object;
//
//         // Act
//         await _context.Publish(eventMessage);
//
//         // Assert
//         Assert.Multiple(() =>
//         {
//             Assert.That(_context.Messages, Has.Count.EqualTo(1));
//         });
//     }
//
//     [Test]
//     public async Task Send_GivenMultipleCommands_EnqueuesAllMessages()
//     {
//         // Arrange
//         var command1 = new Mock<IAmACommand>().Object;
//         var command2 = new Mock<IAmACommand>().Object;
//
//         // Act
//         await _context.Send(command1);
//         await _context.Send(command2);
//
//         // Assert
//         Assert.That(_context.Messages, Has.Count.EqualTo(2));
//     }
//
//     [Test]
//     public async Task Publish_GivenMultipleEvents_EnqueuesAllMessages()
//     {
//         // Arrange
//         var event1 = new Mock<IAmAnEvent>().Object;
//         var event2 = new Mock<IAmAnEvent>().Object;
//
//         // Act
//         await _context.Publish(event1);
//         await _context.Publish(event2);
//
//         // Assert
//         Assert.That(_context.Messages, Has.Count.EqualTo(2));
//     }
// }