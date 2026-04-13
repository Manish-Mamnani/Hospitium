using Hospitium.Contracts.Events;
using MassTransit;
using Hospitium.NotificationService.Services;

namespace Hospitium.NotificationService.Consumers
{
    /// <summary>
    /// MassTransit consumer that handles <see cref="PasswordResetRequestedEvent"/> events by sending an OTP email to the user.
    /// </summary>
    public class PasswordResetRequestedConsumer : IConsumer<PasswordResetRequestedEvent>
    {
        private readonly EmailService _email;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetRequestedConsumer"/> class.
        /// </summary>
        /// <param name="email">The email service for sending notifications.</param>
        public PasswordResetRequestedConsumer(EmailService email)
        {
            _email = email;
        }

        public async Task Consume(ConsumeContext<PasswordResetRequestedEvent> context)
        {
            var evt = context.Message;

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                    <div style='background-color: #4F46E5; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>Password Reset Request</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <h2 style='color: #333;'>Hi {evt.Name},</h2>
                        <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                            We received a request to reset your Hospitium password. Use the OTP code below to proceed.
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='display: inline-block; background-color: #f3f4f6; border: 2px dashed #4F46E5; border-radius: 12px; padding: 20px 40px;'>
                                <p style='margin: 0; font-size: 13px; color: #6B7280; letter-spacing: 1px; text-transform: uppercase;'>Your OTP Code</p>
                                <p style='margin: 8px 0 0 0; font-size: 42px; font-weight: bold; letter-spacing: 12px; color: #4F46E5;'>{evt.Otp}</p>
                            </div>
                        </div>
                        <p style='color: #555; font-size: 14px; line-height: 1.5; text-align: center;'>
                            This code is valid for <strong>10 minutes</strong>. Do not share it with anyone.
                        </p>
                        <p style='color: #555; font-size: 14px; line-height: 1.5;'>
                            If you did not request a password reset, you can safely ignore this email. Your password will not be changed.
                        </p>
                    </div>
                    <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                        &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                    </div>
                </div>";

            await _email.SendEmailAsync(
                evt.Email,
                "🔐 Your Hospitium Password Reset OTP",
                htmlBody
            );
        }
    }
}
