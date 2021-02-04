# Generated by Django 3.1.5 on 2021-02-04 17:51

from django.db import migrations, models
import django.db.models.deletion
import uuid


class Migration(migrations.Migration):

    initial = True

    dependencies = [
        ('apps', '0002_auto_20210108_1311'),
    ]

    operations = [
        migrations.CreateModel(
            name='Function',
            fields=[
                ('uuid', models.UUIDField(auto_created=True, default=uuid.uuid4, editable=False, primary_key=True, serialize=False, unique=True, verbose_name='UUID')),
                ('created', models.DateTimeField(auto_now_add=True, verbose_name='date created')),
                ('updated', models.DateTimeField(auto_now=True, verbose_name='date updated')),
                ('name', models.CharField(max_length=100)),
                ('args', models.CharField(max_length=1000)),
                ('app', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, to='apps.app')),
            ],
        ),
        migrations.CreateModel(
            name='EnvironmentVariable',
            fields=[
                ('uuid', models.UUIDField(auto_created=True, default=uuid.uuid4, editable=False, primary_key=True, serialize=False, unique=True, verbose_name='UUID')),
                ('created', models.DateTimeField(auto_now_add=True, verbose_name='date created')),
                ('updated', models.DateTimeField(auto_now=True, verbose_name='date updated')),
                ('key', models.CharField(max_length=100)),
                ('value', models.CharField(max_length=1000)),
                ('app', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, to='apps.app')),
            ],
        ),
        migrations.AddConstraint(
            model_name='function',
            constraint=models.UniqueConstraint(fields=('app', 'name'), name='function_is_unique'),
        ),
        migrations.AddConstraint(
            model_name='environmentvariable',
            constraint=models.UniqueConstraint(fields=('app', 'key'), name='envvar_is_unique'),
        ),
    ]
