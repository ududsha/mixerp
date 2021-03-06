﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using MixERP.Net.Common;
using MixERP.Net.DbFactory;
using MixERP.Net.Entities;
using MixERP.Net.Entities.Core;
using Npgsql;
using Serilog;

namespace MixERP.Net.Core.Modules.BackOffice.Data
{
    public static class Attachments
    {
        public static string DeleteReturningPath(long id)
        {
            const string sql = "DELETE from core.attachments WHERE attachment_id=@AttachmentId RETURNING file_path;";
            using (NpgsqlCommand command = new NpgsqlCommand(sql))
            {
                command.Parameters.AddWithValue("@AttachmentId", id);

                return Conversion.TryCastString(DbOperation.GetScalarValue(command));
            }
        }

        public static IEnumerable<Attachment> GetAttachments(string attachmentDirectory, string book, long id)
        {
            return Factory.Get<Attachment>("SELECT attachment_id, user_id, resource, resource_key, resource_id, original_file_name, file_extension, @0 || file_path as file_path, comment, added_on FROM core.attachments WHERE resource || resource_key=core.get_attachment_lookup_info(@1) AND resource_id=@2;", attachmentDirectory, book, id);
        }

        public static bool Save(int userId, string book, long id, Collection<Attachment> attachments)
        {
            const string sql = "INSERT INTO core.attachments(user_id, resource, resource_key, resource_id, original_file_name, file_extension, file_path, comment) " +
                               "SELECT @UserId, core.attachment_lookup.resource, core.attachment_lookup.resource_key, @ResourceId, @OriginalFileName, @FileExtension, @FilePath, @Comment" +
                               " FROM core.attachment_lookup WHERE book=@Book;";

            using (NpgsqlConnection connection = new NpgsqlConnection(DbConnection.GetConnectionString()))
            {
                connection.Open();

                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    if (attachments != null && attachments.Count > 0)
                    {
                        try
                        {
                            foreach (Attachment attachment in attachments)
                            {
                                using (NpgsqlCommand attachmentCommand = new NpgsqlCommand(sql, connection))
                                {
                                    attachmentCommand.Parameters.AddWithValue("@UserId", userId);
                                    attachmentCommand.Parameters.AddWithValue("@Book", book);
                                    attachmentCommand.Parameters.AddWithValue("@ResourceId", id);
                                    attachmentCommand.Parameters.AddWithValue("@OriginalFileName", attachment.OriginalFileName);
                                    attachmentCommand.Parameters.AddWithValue("@FileExtension", Path.GetExtension(attachment.OriginalFileName));
                                    attachmentCommand.Parameters.AddWithValue("@FilePath", attachment.FilePath);
                                    attachmentCommand.Parameters.AddWithValue("@Comment", attachment.Comment);

                                    attachmentCommand.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (NpgsqlException)
                        {
                            Log.Warning(@"Could not insert attachment into database. Book: {Book}, Id: {Id}, Attachments: {Attachments}.\n{Sql}", book, id, attachments, sql);

                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }

            return false;
        }
    }
}